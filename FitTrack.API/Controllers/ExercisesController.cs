using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;

namespace FitTrack.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IExerciseDbClient _exerciseDbClient;

    public ExercisesController(
        IExerciseRepository exerciseRepository,
        IExerciseDbClient exerciseDbClient)
    {
        _exerciseRepository = exerciseRepository;
        _exerciseDbClient = exerciseDbClient;
    }

    // GET: api/v1/exercises - Buscar exercícios (públicos + do usuário)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExerciseDto>>> GetExercises([FromQuery] ExerciseSearchDto searchDto)
    {
        // Se usuário está autenticado, incluir seus exercícios também
        var currentUserId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(currentUserId))
        {
            searchDto.UserId = currentUserId;
            searchDto.OnlyPublic = false; // Incluir exercícios do usuário
        }
        else
        {
            searchDto.OnlyPublic = true; // Apenas exercícios públicos
        }

        var exercises = await _exerciseRepository.SearchAsync(searchDto);
        return Ok(exercises.Select(MapToDto));
    }

    // GET: api/v1/exercises/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise(string id)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null)
        {
            return NotFound();
        }

        // Verificar se o exercício é público ou pertence ao usuário
        var currentUserId = GetCurrentUserId();
        if (!exercise.IsPublic && exercise.UserId != currentUserId)
        {
            return Forbid();
        }

        return Ok(MapToDto(exercise));
    }

    // GET: api/v1/exercises/my - Exercícios do usuário atual
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ExerciseDto>>> GetMyExercises()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var exercises = await _exerciseRepository.GetByUserIdAsync(userId);
        return Ok(exercises.Select(MapToDto));
    }

    // POST: api/v1/exercises - Criar exercício customizado
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ExerciseDto>> CreateExercise([FromBody] CreateExerciseDto createDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var exercise = new Exercise
        {
            UserId = userId, // Sempre associar ao usuário autenticado
            Name = createDto.Name,
            BodyPart = createDto.BodyPart,
            TargetMuscle = createDto.TargetMuscle,
            Equipment = createDto.Equipment,
            Instructions = createDto.Instructions,
            SecondaryMuscles = createDto.SecondaryMuscles,
            Difficulty = createDto.Difficulty,
            IsCustom = true,
            IsPublic = createDto.IsPublic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _exerciseRepository.CreateAsync(exercise);

        return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, MapToDto(exercise));
    }

    // PUT: api/v1/exercises/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ExerciseDto>> UpdateExercise(string id, [FromBody] CreateExerciseDto updateDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null)
        {
            return NotFound();
        }

        // Verificar se o exercício pertence ao usuário
        if (exercise.UserId != userId)
        {
            return Forbid();
        }

        exercise.Name = updateDto.Name;
        exercise.BodyPart = updateDto.BodyPart;
        exercise.TargetMuscle = updateDto.TargetMuscle;
        exercise.Equipment = updateDto.Equipment;
        exercise.Instructions = updateDto.Instructions;
        exercise.SecondaryMuscles = updateDto.SecondaryMuscles;
        exercise.Difficulty = updateDto.Difficulty;
        exercise.IsPublic = updateDto.IsPublic;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _exerciseRepository.UpdateAsync(exercise);

        return Ok(MapToDto(exercise));
    }

    // DELETE: api/v1/exercises/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteExercise(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var exercise = await _exerciseRepository.GetByIdAsync(id);
        if (exercise == null)
        {
            return NotFound();
        }

        // Verificar se o exercício pertence ao usuário
        if (exercise.UserId != userId)
        {
            return Forbid();
        }

        await _exerciseRepository.DeleteAsync(id);
        return NoContent();
    }

    // GET: api/v1/exercises/body-parts
    [HttpGet("body-parts")]
    public async Task<ActionResult<IEnumerable<string>>> GetBodyParts()
    {
        var bodyParts = await _exerciseRepository.GetBodyPartsAsync();
        return Ok(bodyParts);
    }

    // GET: api/v1/exercises/target-muscles
    [HttpGet("target-muscles")]
    public async Task<ActionResult<IEnumerable<string>>> GetTargetMuscles()
    {
        var targetMuscles = await _exerciseRepository.GetTargetMusclesAsync();
        return Ok(targetMuscles);
    }

    // GET: api/v1/exercises/equipment
    [HttpGet("equipment")]
    public async Task<ActionResult<IEnumerable<string>>> GetEquipment()
    {
        var equipment = await _exerciseRepository.GetEquipmentListAsync();
        return Ok(equipment);
    }

    // POST: api/v1/exercises/sync - Sincronizar com ExerciseDB (Admin/Usuário)
    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult<object>> SyncWithExerciseDb()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var externalExercises = await _exerciseDbClient.GetExercisesAsync();
            var syncedCount = 0;
            var skippedCount = 0;

            foreach (var externalExercise in externalExercises)
            {
                // Verificar se já existe pelo ExternalId
                if (!string.IsNullOrEmpty(externalExercise.ExternalId) &&
                    await _exerciseRepository.ExternalIdExistsAsync(externalExercise.ExternalId))
                {
                    skippedCount++;
                    continue;
                }

                var exercise = new Exercise
                {
                    UserId = null, // Exercícios da API são globais (UserId null)
                    ExternalId = externalExercise.ExternalId,
                    Name = externalExercise.Name,
                    BodyPart = externalExercise.BodyPart,
                    TargetMuscle = externalExercise.TargetMuscle,
                    Equipment = externalExercise.Equipment,
                    GifUrl = externalExercise.GifUrl,
                    Instructions = externalExercise.Instructions,
                    SecondaryMuscles = externalExercise.SecondaryMuscles,
                    IsCustom = false,
                    IsPublic = true, // Exercícios da API são públicos
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _exerciseRepository.CreateAsync(exercise);
                syncedCount++;
            }

            return Ok(new
            {
                message = "Sincronização concluída",
                synced = syncedCount,
                skipped = skippedCount,
                total = externalExercises.Count,
                userId = userId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro na sincronização", error = ex.Message });
        }
    }

    // GET: api/v1/exercises/external/body-parts
    [HttpGet("external/body-parts")]
    public async Task<ActionResult<IEnumerable<string>>> GetExternalBodyParts()
    {
        var bodyParts = await _exerciseDbClient.GetBodyPartsAsync();
        return Ok(bodyParts);
    }

    // Método auxiliar para obter o ID do usuário atual
    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private ExerciseDto MapToDto(Exercise exercise)
    {
        return new ExerciseDto
        {
            Id = exercise.Id,
            UserId = exercise.UserId,
            ExternalId = exercise.ExternalId,
            Name = exercise.Name,
            BodyPart = exercise.BodyPart,
            TargetMuscle = exercise.TargetMuscle,
            Equipment = exercise.Equipment,
            GifUrl = exercise.GifUrl,
            Instructions = exercise.Instructions,
            SecondaryMuscles = exercise.SecondaryMuscles,
            Difficulty = exercise.Difficulty,
            IsCustom = exercise.IsCustom,
            IsPublic = exercise.IsPublic,
            CreatedAt = exercise.CreatedAt
        };
    }
}