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
public class TrainingSessionsController : ControllerBase
{
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ITrainingPlanService _planService;
    private readonly IUserRepository _userRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public TrainingSessionsController(
        ITrainingSessionRepository sessionRepository,
        ITrainingPlanService planService,
        IUserRepository userRepository,
        IExerciseRepository exerciseRepository)
    {
        _sessionRepository = sessionRepository;
        _planService = planService;
        _userRepository = userRepository;
        _exerciseRepository = exerciseRepository;
    }

    // POST: api/v1/training-sessions
    [HttpPost]
    public async Task<ActionResult<TrainingSessionLogDto>> CreateSessionLog([FromBody] CreateSessionLogDto createDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado" });
        }

        // Calcular calorias se não fornecidas
        var caloriesBurned = createDto.CaloriesBurned > 0 ? createDto.CaloriesBurned :
                           _planService.CalculateCaloriesBurned(user, new TrainingSessionLog
                           {
                               DurationMinutes = createDto.DurationMinutes,
                               SessionType = createDto.SessionType
                           });

        var sessionLog = new TrainingSessionLog
        {
            UserId = userId,
            PlanId = createDto.PlanId,
            SessionId = createDto.SessionId,
            Date = createDto.Date,
            SessionType = createDto.SessionType,
            DurationMinutes = createDto.DurationMinutes,
            CaloriesBurned = caloriesBurned,
            Notes = createDto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Processar exercícios
        foreach (var exerciseDto in createDto.Exercises)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseDto.ExerciseId);
            var sessionExercise = new SessionExerciseLog
            {
                ExerciseId = exerciseDto.ExerciseId,
                ExerciseName = exercise?.Name ?? "Exercício não encontrado",
                Completed = exerciseDto.Completed,
                Sets = exerciseDto.Sets.Select(s => new ExerciseSet
                {
                    SetNumber = s.SetNumber,
                    Reps = s.Reps,
                    WeightKg = s.WeightKg,
                    DurationSeconds = s.DurationSeconds,
                    Completed = s.Completed
                }).ToList()
            };

            sessionLog.Exercises.Add(sessionExercise);
        }

        await _sessionRepository.CreateAsync(sessionLog);
        return CreatedAtAction(nameof(GetSessionLog), new { id = sessionLog.Id }, MapToDto(sessionLog));
    }

    // GET: api/v1/training-sessions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingSessionLogDto>>> GetSessionLogs(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var sessions = await _sessionRepository.GetByUserIdAsync(userId, from, to);
        return Ok(sessions.Select(MapToDto));
    }

    // GET: api/v1/training-sessions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingSessionLogDto>> GetSessionLog(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        if (session.UserId != userId)
        {
            return Forbid();
        }

        return Ok(MapToDto(session));
    }

    // GET: api/v1/training-sessions/plan/{planId}
    [HttpGet("plan/{planId}")]
    public async Task<ActionResult<IEnumerable<TrainingSessionLogDto>>> GetSessionsByPlan(string planId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var sessions = await _sessionRepository.GetByPlanIdAsync(planId);

        // Filtrar apenas sessões do usuário atual
        var userSessions = sessions.Where(s => s.UserId == userId);

        return Ok(userSessions.Select(MapToDto));
    }

    // GET: api/v1/training-sessions/stats
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetSessionStats(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;

        var sessions = await _sessionRepository.GetByUserIdAsync(userId, fromDate, toDate);
        var sessionsList = sessions.ToList();

        return Ok(new
        {
            period = new { from = fromDate, to = toDate },
            totalSessions = sessionsList.Count,
            totalMinutes = sessionsList.Sum(s => s.DurationMinutes),
            totalCalories = sessionsList.Sum(s => s.CaloriesBurned),
            averageSessionDuration = sessionsList.Any() ? sessionsList.Average(s => s.DurationMinutes) : 0,
            sessionsPerWeek = CalculateSessionsPerWeek(sessionsList, fromDate, toDate),
            mostCommonSessionType = GetMostCommonSessionType(sessionsList)
        });
    }

    // DELETE: api/v1/training-sessions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSessionLog(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        if (session.UserId != userId)
        {
            return Forbid();
        }

        await _sessionRepository.DeleteAsync(id);
        return NoContent();
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private TrainingSessionLogDto MapToDto(TrainingSessionLog session)
    {
        return new TrainingSessionLogDto
        {
            Id = session.Id,
            UserId = session.UserId,
            PlanId = session.PlanId,
            SessionId = session.SessionId,
            Date = session.Date,
            SessionType = session.SessionType,
            DurationMinutes = session.DurationMinutes,
            CaloriesBurned = session.CaloriesBurned,
            Notes = session.Notes,
            Exercises = session.Exercises.Select(e => new SessionExerciseLogDto
            {
                ExerciseId = e.ExerciseId,
                ExerciseName = e.ExerciseName,
                Completed = e.Completed,
                Sets = e.Sets.Select(s => new ExerciseSetDto
                {
                    SetNumber = s.SetNumber,
                    Reps = s.Reps,
                    WeightKg = s.WeightKg,
                    DurationSeconds = s.DurationSeconds,
                    Completed = s.Completed
                }).ToList()
            }).ToList(),
            CreatedAt = session.CreatedAt
        };
    }

    private double CalculateSessionsPerWeek(List<TrainingSessionLog> sessions, DateTime from, DateTime to)
    {
        var totalWeeks = Math.Max((to - from).TotalDays / 7, 1);
        return sessions.Count / totalWeeks;
    }

    private string GetMostCommonSessionType(List<TrainingSessionLog> sessions)
    {
        return sessions
            .GroupBy(s => s.SessionType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "mixed";
    }
}