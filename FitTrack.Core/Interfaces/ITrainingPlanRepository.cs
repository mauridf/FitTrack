using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface ITrainingPlanRepository
{
    Task<TrainingPlan?> GetByIdAsync(string id);
    Task<IEnumerable<TrainingPlan>> GetByUserIdAsync(string userId);
    Task<TrainingPlan> CreateAsync(TrainingPlan plan);
    Task UpdateAsync(TrainingPlan plan);
    Task<bool> DeleteAsync(string id);
    Task<TrainingPlan?> GetActivePlanAsync(string userId);
}

public interface ITrainingSessionRepository
{
    Task<TrainingSessionLog?> GetByIdAsync(string id);
    Task<IEnumerable<TrainingSessionLog>> GetByUserIdAsync(string userId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<TrainingSessionLog>> GetByPlanIdAsync(string planId);
    Task<TrainingSessionLog> CreateAsync(TrainingSessionLog session);
    Task UpdateAsync(TrainingSessionLog session);
    Task<bool> DeleteAsync(string id);
    Task<int> GetCompletedSessionsCountAsync(string userId, DateTime from, DateTime to);
}