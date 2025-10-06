using MongoDB.Driver;
using FitTrack.Core.Entities;
using FitTrack.Core.Interfaces;
using FitTrack.Infrastructure.Data;

namespace FitTrack.Infrastructure.Repositories;

public class MeasurementRepository : IMeasurementRepository
{
    private readonly MongoDbContext _context;

    public MeasurementRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Measurement?> GetByIdAsync(string id)
    {
        return await _context.Measurements.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Measurement>> GetByUserIdAsync(string userId, string? type = null, DateTime? from = null, DateTime? to = null)
    {
        var filterBuilder = Builders<Measurement>.Filter;
        var filter = filterBuilder.Eq(m => m.UserId, userId);

        if (!string.IsNullOrEmpty(type))
        {
            filter &= filterBuilder.Eq(m => m.Type, type);
        }

        if (from.HasValue)
        {
            filter &= filterBuilder.Gte(m => m.Date, from.Value);
        }

        if (to.HasValue)
        {
            filter &= filterBuilder.Lte(m => m.Date, to.Value);
        }

        return await _context.Measurements
            .Find(filter)
            .SortByDescending(m => m.Date)
            .ToListAsync();
    }

    public async Task<Measurement> CreateAsync(Measurement measurement)
    {
        await _context.Measurements.InsertOneAsync(measurement);
        return measurement;
    }

    public async Task UpdateAsync(Measurement measurement)
    {
        await _context.Measurements.ReplaceOneAsync(m => m.Id == measurement.Id, measurement);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Measurements.DeleteOneAsync(m => m.Id == id);
        return result.DeletedCount > 0;
    }
}