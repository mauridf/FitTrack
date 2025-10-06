using Microsoft.AspNetCore.Mvc;
using System.Net;
using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;

namespace FitTrack.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] CreateUserDto createUserDto)
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

        // Gerar tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(GetIpAddress());
        refreshToken.UserId = user.Id!;

        await _refreshTokenRepository.CreateAsync(refreshToken);

        var userDto = MapToUserDto(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutos
            User = userDto
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Email ou senha inválidos." });
        }

        // Gerar tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(GetIpAddress());
        refreshToken.UserId = user.Id!;

        await _refreshTokenRepository.CreateAsync(refreshToken);

        var userDto = MapToUserDto(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = userDto
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null)
        {
            return Unauthorized(new { message = "Usuário não encontrado." });
        }

        // Revogar token atual e gerar novos
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = GetIpAddress();
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(GetIpAddress());
        newRefreshToken.UserId = user.Id!;

        await _refreshTokenRepository.CreateAsync(newRefreshToken);

        var userDto = MapToUserDto(user);

        return Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = userDto
        });
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return BadRequest(new { message = "Token inválido." });
        }

        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = GetIpAddress();
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        return Ok(new { message = "Token revogado com sucesso." });
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"]!;
        else
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }

    private UserDto MapToUserDto(User user)
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