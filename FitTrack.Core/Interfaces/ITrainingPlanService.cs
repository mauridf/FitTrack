using FitTrack.Core.DTOs;
using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface ITrainingPlanService
{
    Task<TrainingPlan> GenerateTrainingPlanAsync(string userId, CreateTrainingPlanDto createDto);
    Task<TrainingPlan> RegenerateWeekAsync(TrainingPlan plan, int weekNumber);
    double CalculateProgress(TrainingPlan plan);
    int CalculateCaloriesBurned(User user, TrainingSessionLog session);
}