using MongoDB.Driver;
using FitTrack.Core.Entities;
using FitTrack.Core.Interfaces;
using FitTrack.Infrastructure.Data;

namespace FitTrack.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly MongoDbContext _context;

    public RefreshTokenRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Find(rt => rt.Token == token)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(RefreshToken token)
    {
        await _context.RefreshTokens.InsertOneAsync(token);
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        await _context.RefreshTokens.ReplaceOneAsync(rt => rt.Id == token.Id, token);
    }

    public async Task RevokeDescendantTokensAsync(RefreshToken token, string ipAddress, string reason)
    {
        if (!string.IsNullOrEmpty(token.ReplacedByToken))
        {
            var childToken = await GetByTokenAsync(token.ReplacedByToken);
            if (childToken != null && childToken.IsActive)
            {
                childToken.Revoked = DateTime.UtcNow;
                childToken.RevokedByIp = ipAddress;
                await UpdateAsync(childToken);
                await RevokeDescendantTokensAsync(childToken, ipAddress, reason);
            }
        }
    }

    public async Task RevokeSimilarTokensAsync(RefreshToken token, string ipAddress, string reason)
    {
        // Revoga todos os tokens similares do mesmo usuário
        var similarTokens = await _context.RefreshTokens
            .Find(rt => rt.UserId == token.UserId && rt.IsActive)
            .ToListAsync();

        foreach (var similarToken in similarTokens)
        {
            similarToken.Revoked = DateTime.UtcNow;
            similarToken.RevokedByIp = ipAddress;
            await UpdateAsync(similarToken);
        }
    }
}