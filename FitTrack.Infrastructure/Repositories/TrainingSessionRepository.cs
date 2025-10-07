using MongoDB.Driver;
using FitTrack.Core.Entities;
using FitTrack.Core.Interfaces;
using FitTrack.Infrastructure.Data;

namespace FitTrack.Infrastructure.Repositories;

public class TrainingSessionRepository : ITrainingSessionRepository
{
    private readonly MongoDbContext _context;

    public TrainingSessionRepository(MongoDbContext context)
    {
        _context = context;
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Índice para UserId + Date
        var indexKeys = Builders<TrainingSessionLog>.IndexKeys
            .Ascending(s => s.UserId)
            .Descending(s => s.Date);

        var indexOptions = new CreateIndexOptions { Name = "UserId_Date_Index" };
        var indexModel = new CreateIndexModel<TrainingSessionLog>(indexKeys, indexOptions);

        _context.TrainingSessions.Indexes.CreateOne(indexModel);
    }

    public async Task<TrainingSessionLog?> GetByIdAsync(string id)
    {
        return await _context.TrainingSessions.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TrainingSessionLog>> GetByUserIdAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var filterBuilder = Builders<TrainingSessionLog>.Filter;
        var filter = filterBuilder.Eq(s => s.UserId, userId);

        if (from.HasValue)
        {
            filter &= filterBuilder.Gte(s => s.Date, from.Value);
        }

        if (to.HasValue)
        {
            filter &= filterBuilder.Lte(s => s.Date, to.Value);
        }

        return await _context.TrainingSessions
            .Find(filter)
            .SortByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingSessionLog>> GetByPlanIdAsync(string planId)
    {
        return await _context.TrainingSessions
            .Find(s => s.PlanId == planId)
            .SortByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<TrainingSessionLog> CreateAsync(TrainingSessionLog session)
    {
        await _context.TrainingSessions.InsertOneAsync(session);
        return session;
    }

    public async Task UpdateAsync(TrainingSessionLog session)
    {
        await _context.TrainingSessions.ReplaceOneAsync(s => s.Id == session.Id, session);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.TrainingSessions.DeleteOneAsync(s => s.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<int> GetCompletedSessionsCountAsync(string userId, DateTime from, DateTime to)
    {
        var filterBuilder = Builders<TrainingSessionLog>.Filter;
        var filter = filterBuilder.Eq(s => s.UserId, userId) &
                    filterBuilder.Gte(s => s.Date, from) &
                    filterBuilder.Lte(s => s.Date, to);

        return (int)await _context.TrainingSessions.CountDocumentsAsync(filter);
    }
}