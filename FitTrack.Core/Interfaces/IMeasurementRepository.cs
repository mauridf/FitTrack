using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface IMeasurementRepository
{
    Task<Measurement?> GetByIdAsync(string id);
    Task<IEnumerable<Measurement>> GetByUserIdAsync(string userId, string? type = null, DateTime? from = null, DateTime? to = null);
    Task<Measurement> CreateAsync(Measurement measurement);
    Task UpdateAsync(Measurement measurement);
    Task<bool> DeleteAsync(string id);
}