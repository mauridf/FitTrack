using Microsoft.AspNetCore.Mvc;
using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;

namespace FitTrack.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICalculationService _calculationService;

    public UsersController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICalculationService calculationService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _calculationService = calculationService;
    }

    // GET: api/v1/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(user));
    }

    // POST: api/v1/users
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        // Verificar se email já existe
        if (await _userRepository.EmailExistsAsync(createUserDto.Email))
        {
            return BadRequest(new { message = "Email já está em uso." });
        }

        // Criar usuário
        var user = new User
        {
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = _passwordHasher.HashPassword(createUserDto.Password),
            BirthDate = createUserDto.BirthDate,
            Gender = createUserDto.Gender,
            HeightCm = createUserDto.HeightCm,
            WeightKg = createUserDto.WeightKg,
            ActivityLevel = createUserDto.ActivityLevel,
            Goal = createUserDto.Goal,
            TargetWeightKg = createUserDto.TargetWeightKg,
            AvailableEquipment = createUserDto.AvailableEquipment,
            Restrictions = createUserDto.Restrictions,
            Preference = createUserDto.Preference,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToDto(user));
    }

    // PUT: api/v1/users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Atualizar campos permitidos
        user.Name = updateUserDto.Name;
        user.BirthDate = updateUserDto.BirthDate;
        user.Gender = updateUserDto.Gender;
        user.HeightCm = updateUserDto.HeightCm;
        user.WeightKg = updateUserDto.WeightKg;
        user.ActivityLevel = updateUserDto.ActivityLevel;
        user.Goal = updateUserDto.Goal;
        user.TargetWeightKg = updateUserDto.TargetWeightKg;
        user.AvailableEquipment = updateUserDto.AvailableEquipment;
        user.Restrictions = updateUserDto.Restrictions;
        user.Preference = updateUserDto.Preference;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return Ok(MapToDto(user));
    }

    // GET: api/v1/users/{id}/calculations
    [HttpGet("{id}/calculations")]
    public async Task<ActionResult<object>> GetUserCalculations(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var bmi = _calculationService.CalculateBMI(user.WeightKg, user.HeightCm);
        var bmiClassification = _calculationService.ClassifyBMI(bmi);
        var idealWeight = _calculationService.CalculateIdealWeight(user.HeightCm, user.Gender);

        return Ok(new
        {
            bmi,
            bmiClassification,
            idealWeight,
            currentWeight = user.WeightKg,
            targetWeight = user.TargetWeightKg,
            weightToLoseOrGain = Math.Round(user.WeightKg - user.TargetWeightKg, 1)
        });
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            HeightCm = user.HeightCm,
            WeightKg = user.WeightKg,
            ActivityLevel = user.ActivityLevel,
            Goal = user.Goal,
            TargetWeightKg = user.TargetWeightKg,
            AvailableEquipment = user.AvailableEquipment,
            Restrictions = user.Restrictions,
            Preference = user.Preference,
            CreatedAt = user.CreatedAt
        };
    }
}