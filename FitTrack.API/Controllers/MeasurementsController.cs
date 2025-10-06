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
public class MeasurementsController : ControllerBase
{
    private readonly IMeasurementRepository _measurementRepository;
    private readonly IUserRepository _userRepository;

    public MeasurementsController(
        IMeasurementRepository measurementRepository,
        IUserRepository userRepository)
    {
        _measurementRepository = measurementRepository;
        _userRepository = userRepository;
    }

    // POST: api/v1/measurements
    [HttpPost]
    public async Task<ActionResult<MeasurementDto>> CreateMeasurement([FromBody] CreateMeasurementDto createDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Verificar se o usuário existe
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        var measurement = new Measurement
        {
            UserId = userId,
            Type = createDto.Type,
            Value = createDto.Value,
            Unit = createDto.Unit,
            Date = createDto.Date ?? DateTime.UtcNow
        };

        await _measurementRepository.CreateAsync(measurement);

        // Se for medida de peso, atualizar o peso atual do usuário
        if (createDto.Type == "weight")
        {
            user.WeightKg = createDto.Value;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        return CreatedAtAction(nameof(GetMeasurement), new { id = measurement.Id }, MapToDto(measurement));
    }

    // GET: api/v1/measurements
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MeasurementDto>>> GetMeasurements(
        [FromQuery] string? type = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var measurements = await _measurementRepository.GetByUserIdAsync(userId, type, from, to);

        return Ok(measurements.Select(MapToDto));
    }

    // GET: api/v1/measurements/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<MeasurementDto>> GetMeasurement(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var measurement = await _measurementRepository.GetByIdAsync(id);
        if (measurement == null)
        {
            return NotFound();
        }

        // Verificar se a medida pertence ao usuário
        if (measurement.UserId != userId)
        {
            return Forbid();
        }

        return Ok(MapToDto(measurement));
    }

    // DELETE: api/v1/measurements/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeasurement(string id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var measurement = await _measurementRepository.GetByIdAsync(id);
        if (measurement == null)
        {
            return NotFound();
        }

        // Verificar se a medida pertence ao usuário
        if (measurement.UserId != userId)
        {
            return Forbid();
        }

        await _measurementRepository.DeleteAsync(id);
        return NoContent();
    }

    // GET: api/v1/measurements/weight/progress
    [HttpGet("weight/progress")]
    public async Task<ActionResult<object>> GetWeightProgress([FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var fromDate = DateTime.UtcNow.AddDays(-days);
        var weightMeasurements = await _measurementRepository.GetByUserIdAsync(userId, "weight", fromDate);

        var progressData = weightMeasurements
            .OrderBy(m => m.Date)
            .Select(m => new
            {
                date = m.Date.ToString("yyyy-MM-dd"),
                weight = m.Value,
                unit = m.Unit
            })
            .ToList();

        return Ok(new
        {
            periodDays = days,
            measurements = progressData,
            totalMeasurements = progressData.Count
        });
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private MeasurementDto MapToDto(Measurement measurement)
    {
        return new MeasurementDto
        {
            Id = measurement.Id,
            UserId = measurement.UserId,
            Type = measurement.Type,
            Value = measurement.Value,
            Unit = measurement.Unit,
            Date = measurement.Date
        };
    }
}