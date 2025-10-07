using MongoDB.Driver;
using FitTrack.Core.Entities;
using FitTrack.Core.Interfaces;
using FitTrack.Infrastructure.Data;

namespace FitTrack.Infrastructure.Repositories;

public class TrainingPlanRepository : ITrainingPlanRepository
{
    private readonly MongoDbContext _context;

    public TrainingPlanRepository(MongoDbContext context)
    {
        _context = context;
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Índice para UserId + Status
        var indexKeys = Builders<TrainingPlan>.IndexKeys
            .Ascending(p => p.UserId)
            .Ascending(p => p.Status);

        var indexOptions = new CreateIndexOptions { Name = "UserId_Status_Index" };
        var indexModel = new CreateIndexModel<TrainingPlan>(indexKeys, indexOptions);

        _context.TrainingPlans.Indexes.CreateOne(indexModel);
    }

    public async Task<TrainingPlan?> GetByIdAsync(string id)
    {
        return await _context.TrainingPlans.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TrainingPlan>> GetByUserIdAsync(string userId)
    {
        return await _context.TrainingPlans
            .Find(p => p.UserId == userId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<TrainingPlan> CreateAsync(TrainingPlan plan)
    {
        await _context.TrainingPlans.InsertOneAsync(plan);
        return plan;
    }

    public async Task UpdateAsync(TrainingPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.TrainingPlans.ReplaceOneAsync(p => p.Id == plan.Id, plan);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.TrainingPlans.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<TrainingPlan?> GetActivePlanAsync(string userId)
    {
        return await _context.TrainingPlans
            .Find(p => p.UserId == userId && p.Status == "active")
            .SortByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }
}