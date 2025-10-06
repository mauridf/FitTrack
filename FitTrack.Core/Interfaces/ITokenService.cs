using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(string ipAddress);
}