using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task CreateAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task RevokeDescendantTokensAsync(RefreshToken token, string ipAddress, string reason);
    Task RevokeSimilarTokensAsync(RefreshToken token, string ipAddress, string reason);
}