using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;

namespace FitTrack.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TrainingPlansController : ControllerBase
{
    private readonly ITrainingPlanRepository _planRepository;
    private readonly ITrainingPlanService _planService;
    private readonly IUserRepository _userRepository;

    public TrainingPlansController(
        ITrainingPlanRepository planRepository,
        ITrainingPlanService planService,
        IUserRepository userRepository)
    {
        _planRepository = planRepository;
        _planService = planService;
        _userRepository = userRepository;
    }

    // GET: api/v1/training-plans
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingPlanDto>>> GetTrainingPlans()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plans = await _planRepository.GetByUserIdAsync(userId);
        return Ok(plans.Select(MapToDto));
    }

    // GET: api/v1/training-plans/active
    [HttpGet("active")]
    public async Task<ActionResult<TrainingPlanDto>> GetActivePlan()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _planRepository.GetActivePlanAsync(userId);
        if (plan == null)
        {
            return NotFound(new { message = "Nenhum plano ativo encontrado" });
        }

        return Ok(MapToDto(plan));
    }

    // GET: api/v1/training-plans/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingPlanDto>> GetTrainingPlan(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound();
        }

        if (plan.UserId != userId)
        {
            return Forbid();
        }

        return Ok(MapToDto(plan));
    }

    // POST: api/v1/training-plans/generate
    [HttpPost("generate")]
    public async Task<ActionResult<TrainingPlanDto>> GenerateTrainingPlan([FromBody] CreateTrainingPlanDto createDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // Desativar planos ativos existentes
            var activePlans = await _planRepository.GetByUserIdAsync(userId);
            foreach (var activePlan in activePlans.Where(p => p.Status == "active"))
            {
                activePlan.Status = "completed";
                await _planRepository.UpdateAsync(activePlan);
            }

            var plan = await _planService.GenerateTrainingPlanAsync(userId, createDto);
            return CreatedAtAction(nameof(GetTrainingPlan), new { id = plan.Id }, MapToDto(plan));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Erro ao gerar plano de treino", error = ex.Message });
        }
    }

    // PUT: api/v1/training-plans/{id}/weeks/{weekNumber}/regenerate
    [HttpPut("{id}/weeks/{weekNumber}/regenerate")]
    public async Task<ActionResult<TrainingPlanDto>> RegenerateWeek(string id, int weekNumber)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound();
        }

        if (plan.UserId != userId)
        {
            return Forbid();
        }

        if (weekNumber < 1 || weekNumber > plan.DurationWeeks)
        {
            return BadRequest(new { message = "Número da semana inválido" });
        }

        try
        {
            var updatedPlan = await _planService.RegenerateWeekAsync(plan, weekNumber);
            return Ok(MapToDto(updatedPlan));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Erro ao regenerar semana", error = ex.Message });
        }
    }

    // PUT: api/v1/training-plans/{id}/sessions/{sessionId}/complete
    [HttpPut("{id}/sessions/{sessionId}/complete")]
    public async Task<ActionResult<TrainingPlanDto>> CompleteSession(string id, string sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound();
        }

        if (plan.UserId != userId)
        {
            return Forbid();
        }

        // Encontrar a sessão em todas as semanas
        var session = plan.Weeks
            .SelectMany(w => w.Sessions)
            .FirstOrDefault(s => s.Id == sessionId);

        if (session == null)
        {
            return NotFound(new { message = "Sessão não encontrada" });
        }

        session.Status = "completed";
        plan.UpdatedAt = DateTime.UtcNow;

        await _planRepository.UpdateAsync(plan);

        return Ok(MapToDto(plan));
    }

    // DELETE: api/v1/training-plans/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrainingPlan(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound();
        }

        if (plan.UserId != userId)
        {
            return Forbid();
        }

        await _planRepository.DeleteAsync(id);
        return NoContent();
    }

    // GET: api/v1/training-plans/{id}/progress
    [HttpGet("{id}/progress")]
    public async Task<ActionResult<object>> GetPlanProgress(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _planRepository.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound();
        }

        if (plan.UserId != userId)
        {
            return Forbid();
        }

        var progress = _planService.CalculateProgress(plan);
        var totalSessions = plan.Weeks.Sum(w => w.Sessions.Count);
        var completedSessions = plan.Weeks.Sum(w => w.Sessions.Count(s => s.Status == "completed"));
        var remainingWeeks = (plan.EndDate - DateTime.UtcNow).TotalDays / 7;

        return Ok(new
        {
            progressPercentage = Math.Round(progress, 1),
            completedSessions,
            totalSessions,
            remainingSessions = totalSessions - completedSessions,
            estimatedRemainingWeeks = Math.Ceiling(remainingWeeks),
            startDate = plan.StartDate,
            endDate = plan.EndDate,
            currentWeek = GetCurrentWeek(plan)
        });
    }

    private int GetCurrentWeek(TrainingPlan plan)
    {
        var totalDays = (DateTime.UtcNow - plan.StartDate).TotalDays;
        return Math.Min((int)(totalDays / 7) + 1, plan.DurationWeeks);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private TrainingPlanDto MapToDto(TrainingPlan plan)
    {
        return new TrainingPlanDto
        {
            Id = plan.Id,
            UserId = plan.UserId,
            Name = plan.Name,
            Description = plan.Description,
            Goal = plan.Goal,
            Focus = plan.Focus,
            DurationWeeks = plan.DurationWeeks,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            Weeks = plan.Weeks.Select(w => new TrainingWeekDto
            {
                WeekNumber = w.WeekNumber,
                Sessions = w.Sessions.Select(s => new TrainingSessionDto
                {
                    Id = s.Id,
                    Date = s.Date,
                    DayOfWeek = s.DayOfWeek,
                    Type = s.Type,
                    DurationMinutes = s.DurationMinutes,
                    Status = s.Status,
                    Exercises = s.Exercises.Select(e => new SessionExerciseDto
                    {
                        ExerciseId = e.ExerciseId,
                        ExerciseName = e.ExerciseName,
                        Sets = e.Sets,
                        Reps = e.Reps,
                        WeightKg = e.WeightKg,
                        DurationSeconds = e.DurationSeconds,
                        RestSeconds = e.RestSeconds
                    }).ToList()
                }).ToList()
            }).ToList(),
            Status = plan.Status,
            CreatedAt = plan.CreatedAt
        };
    }
}