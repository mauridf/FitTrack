using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;

namespace FitTrack.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMeasurementRepository _measurementRepository;
    private readonly ITrainingSessionRepository _sessionRepository;
    private readonly ITrainingPlanRepository _planRepository;
    private readonly ITrainingPlanService _planService;
    private readonly ICalculationService _calculationService;

    public DashboardController(
        IUserRepository userRepository,
        IMeasurementRepository measurementRepository,
        ITrainingSessionRepository sessionRepository,
        ITrainingPlanRepository planRepository,
        ITrainingPlanService planService,
        ICalculationService calculationService)
    {
        _userRepository = userRepository;
        _measurementRepository = measurementRepository;
        _sessionRepository = sessionRepository;
        _planRepository = planRepository;
        _planService = planService;
        _calculationService = calculationService;
    }

    // GET: api/v1/dashboard/overview
    [HttpGet("overview")]
    public async Task<ActionResult<object>> GetOverview()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var activePlan = await _planRepository.GetActivePlanAsync(userId);
        var recentMeasurements = await _measurementRepository.GetByUserIdAsync(userId, "weight", DateTime.UtcNow.AddDays(-30));
        var weeklySessions = await GetWeeklySessionStats(userId);

        var bmi = _calculationService.CalculateBMI(user.WeightKg, user.HeightCm);
        var idealWeight = _calculationService.CalculateIdealWeight(user.HeightCm, user.Gender);
        var progressPercentage = CalculateWeightProgress(user.WeightKg, user.TargetWeightKg, user.Goal);

        return Ok(new
        {
            user = new
            {
                name = user.Name,
                currentWeight = user.WeightKg,
                targetWeight = user.TargetWeightKg,
                goal = user.Goal,
                bmi = bmi,
                bmiClassification = _calculationService.ClassifyBMI(bmi),
                idealWeight = idealWeight
            },
            progress = new
            {
                percentage = progressPercentage,
                weightToGo = Math.Abs(user.WeightKg - user.TargetWeightKg),
                trend = GetWeightTrend(recentMeasurements),
                estimatedWeeksToGoal = CalculateWeeksToGoal(user.WeightKg, user.TargetWeightKg, user.Goal)
            },
            weeklyStats = weeklySessions,
            activePlan = activePlan != null ? new
            {
                name = activePlan.Name,
                progress = _planService.CalculateProgress(activePlan),
                nextSession = GetNextSession(activePlan)
            } : null,
            recommendations = await GenerateRecommendations(user, activePlan, weeklySessions)
        });
    }

    // GET: api/v1/dashboard/weight-progress
    [HttpGet("weight-progress")]
    public async Task<ActionResult<object>> GetWeightProgress([FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var fromDate = DateTime.UtcNow.AddDays(-days);
        var measurements = await _measurementRepository.GetByUserIdAsync(userId, "weight", fromDate);
        var user = await _userRepository.GetByIdAsync(userId);

        var progressData = measurements
            .OrderBy(m => m.Date)
            .Select(m => new WeightProgressItem
            {
                Date = m.Date,
                Weight = m.Value,
                Bmi = _calculationService.CalculateBMI(m.Value, user?.HeightCm ?? 0)
            })
            .ToList();

        return Ok(new
        {
            periodDays = days,
            measurements = progressData.Select(p => new
            {
                date = p.Date.ToString("yyyy-MM-dd"),
                weight = p.Weight,
                bmi = p.Bmi
            }),
            currentWeight = progressData.LastOrDefault()?.Weight,
            startingWeight = progressData.FirstOrDefault()?.Weight,
            totalChange = progressData.Any() ? progressData.Last().Weight - progressData.First().Weight : 0,
            averageWeeklyChange = CalculateAverageWeeklyChange(progressData)
        });
    }

    // Classe auxiliar para tipagem forte
    private class WeightProgressItem
    {
        public DateTime Date { get; set; }
        public double Weight { get; set; }
        public double Bmi { get; set; }
    }

    private double CalculateAverageWeeklyChange(List<WeightProgressItem> progressData)
    {
        if (progressData.Count < 2) return 0;

        var firstWeight = progressData.First().Weight;
        var lastWeight = progressData.Last().Weight;
        var weeks = Math.Max((progressData.Last().Date - progressData.First().Date).TotalDays / 7, 1);

        return (lastWeight - firstWeight) / weeks;
    }

    // GET: api/v1/dashboard/workout-stats
    [HttpGet("workout-stats")]
    public async Task<ActionResult<object>> GetWorkoutStats([FromQuery] int weeks = 4)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var fromDate = DateTime.UtcNow.AddDays(-weeks * 7);
        var sessions = await _sessionRepository.GetByUserIdAsync(userId, fromDate);
        var sessionsList = sessions.ToList();

        var weeklyBreakdown = GetWeeklyBreakdown(sessionsList, weeks);
        var typeDistribution = GetSessionTypeDistribution(sessionsList);
        var performanceTrend = GetPerformanceTrend(sessionsList, weeks);

        return Ok(new
        {
            periodWeeks = weeks,
            summary = new
            {
                totalSessions = sessionsList.Count,
                totalMinutes = sessionsList.Sum(s => s.DurationMinutes),
                totalCalories = sessionsList.Sum(s => s.CaloriesBurned),
                averageSessionDuration = sessionsList.Any() ? sessionsList.Average(s => s.DurationMinutes) : 0,
                sessionsPerWeek = (double)sessionsList.Count / weeks
            },
            weeklyBreakdown = weeklyBreakdown,
            typeDistribution = typeDistribution,
            performanceTrend = performanceTrend
        });
    }

    // GET: api/v1/dashboard/goal-progress
    [HttpGet("goal-progress")]
    public async Task<ActionResult<object>> GetGoalProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var activePlan = await _planRepository.GetActivePlanAsync(userId);
        var monthlySessions = await _sessionRepository.GetByUserIdAsync(userId, DateTime.UtcNow.AddDays(-30));

        return Ok(new
        {
            weightGoal = new
            {
                current = user.WeightKg,
                target = user.TargetWeightKg,
                progress = CalculateWeightProgress(user.WeightKg, user.TargetWeightKg, user.Goal),
                trend = await GetMonthlyWeightTrend(userId)
            },
            activityGoal = new
            {
                currentSessions = monthlySessions.Count(),
                targetSessions = GetTargetSessions(user.ActivityLevel),
                progress = Math.Min((double)monthlySessions.Count() / GetTargetSessions(user.ActivityLevel) * 100, 100)
            },
            planProgress = activePlan != null ? new
            {
                planName = activePlan.Name,
                progress = _planService.CalculateProgress(activePlan),
                completedSessions = activePlan.Weeks.Sum(w => w.Sessions.Count(s => s.Status == "completed")),
                totalSessions = activePlan.Weeks.Sum(w => w.Sessions.Count)
            } : null
        });
    }

    // GET: api/v1/dashboard/quick-stats
    [HttpGet("quick-stats")]
    public async Task<ActionResult<object>> GetQuickStats()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var todaySessions = await _sessionRepository.GetByUserIdAsync(userId, today, today.AddDays(1));
        var weekSessions = await _sessionRepository.GetByUserIdAsync(userId, weekStart, weekStart.AddDays(7));
        var monthSessions = await _sessionRepository.GetByUserIdAsync(userId, monthStart, monthStart.AddMonths(1));

        var user = await _userRepository.GetByIdAsync(userId);
        var activePlan = await _planRepository.GetActivePlanAsync(userId);

        return Ok(new
        {
            today = new
            {
                sessions = todaySessions.Count(),
                calories = todaySessions.Sum(s => s.CaloriesBurned),
                minutes = todaySessions.Sum(s => s.DurationMinutes)
            },
            thisWeek = new
            {
                sessions = weekSessions.Count(),
                calories = weekSessions.Sum(s => s.CaloriesBurned),
                minutes = weekSessions.Sum(s => s.DurationMinutes)
            },
            thisMonth = new
            {
                sessions = monthSessions.Count(),
                calories = monthSessions.Sum(s => s.CaloriesBurned),
                minutes = monthSessions.Sum(s => s.DurationMinutes)
            },
            currentPlan = activePlan?.Name ?? "Nenhum plano ativo",
            nextSession = activePlan != null ? GetNextSession(activePlan) : null
        });
    }

    // Métodos auxiliares privados
    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private async Task<object> GetWeeklySessionStats(string userId)
    {
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var weekSessions = await _sessionRepository.GetByUserIdAsync(userId, weekStart, weekStart.AddDays(7));
        var sessionsList = weekSessions.ToList();

        return new
        {
            completedSessions = sessionsList.Count,
            totalCalories = sessionsList.Sum(s => s.CaloriesBurned),
            totalMinutes = sessionsList.Sum(s => s.DurationMinutes),
            averageSessionDuration = sessionsList.Any() ? sessionsList.Average(s => s.DurationMinutes) : 0
        };
    }

    private double CalculateWeightProgress(double currentWeight, double targetWeight, string goal)
    {
        if (goal == "perder" && currentWeight > targetWeight)
        {
            var totalToLose = currentWeight - targetWeight;
            var lostSoFar = 0; // Simplificado - poderia usar histórico
            return Math.Min((lostSoFar / totalToLose) * 100, 100);
        }
        else if (goal == "ganhar" && currentWeight < targetWeight)
        {
            var totalToGain = targetWeight - currentWeight;
            var gainedSoFar = 0; // Simplificado
            return Math.Min((gainedSoFar / totalToGain) * 100, 100);
        }
        return 100;
    }

    private string GetWeightTrend(IEnumerable<Measurement> recentMeasurements)
    {
        var ordered = recentMeasurements.OrderBy(m => m.Date).ToList();
        if (ordered.Count < 2) return "stable";

        var first = ordered.First().Value;
        var last = ordered.Last().Value;
        var difference = last - first;

        if (Math.Abs(difference) < 0.5) return "stable";
        return difference > 0 ? "gaining" : "losing";
    }

    private int CalculateWeeksToGoal(double currentWeight, double targetWeight, string goal)
    {
        var difference = Math.Abs(currentWeight - targetWeight);
        var weeklyRate = goal == "perder" ? 0.5 : 0.25; // kg por semana
        return (int)Math.Ceiling(difference / weeklyRate);
    }

    private object GetNextSession(TrainingPlan plan)
    {
        var nextSession = plan.Weeks
            .SelectMany(w => w.Sessions)
            .Where(s => s.Status == "planned" && s.Date >= DateTime.UtcNow.Date)
            .OrderBy(s => s.Date)
            .FirstOrDefault();

        return nextSession != null ? new
        {
            date = nextSession.Date,
            type = nextSession.Type,
            duration = nextSession.DurationMinutes,
            exercisesCount = nextSession.Exercises.Count
        } : null;
    }

    private async Task<List<string>> GenerateRecommendations(User user, TrainingPlan? activePlan, object weeklyStats)
    {
        var recommendations = new List<string>();

        // Recomendações baseadas no progresso do peso
        var weightProgress = CalculateWeightProgress(user.WeightKg, user.TargetWeightKg, user.Goal);
        if (weightProgress < 30)
        {
            recommendations.Add("Consistência é key! Tente manter pelo menos 3 treinos por semana.");
        }

        // Recomendações baseadas em atividade
        var weeklySessions = await _sessionRepository.GetByUserIdAsync(userId: user.Id!, from: DateTime.UtcNow.AddDays(-7));
        if (weeklySessions.Count() < 2)
        {
            recommendations.Add("Aumente a frequência de treinos para ver resultados mais rápidos.");
        }

        // Recomendações baseadas no plano
        if (activePlan == null)
        {
            recommendations.Add("Crie um plano de treino personalizado para melhor acompanhamento.");
        }

        return recommendations.Take(3).ToList();
    }

    private double CalculateAverageWeeklyChange(List<dynamic> progressData)
    {
        if (progressData.Count < 2) return 0;

        var firstWeight = (double)progressData.First().weight;
        var lastWeight = (double)progressData.Last().weight;
        var weeks = Math.Max((progressData.Last().date - progressData.First().date).TotalDays / 7, 1);

        return (lastWeight - firstWeight) / weeks;
    }

    private List<object> GetWeeklyBreakdown(List<TrainingSessionLog> sessions, int weeks)
    {
        var breakdown = new List<object>();
        for (int i = weeks - 1; i >= 0; i--)
        {
            var weekStart = DateTime.UtcNow.AddDays(-i * 7);
            var weekEnd = weekStart.AddDays(7);
            var weekSessions = sessions.Where(s => s.Date >= weekStart && s.Date < weekEnd).ToList();

            breakdown.Add(new
            {
                week = weekStart.ToString("MMM dd"),
                sessions = weekSessions.Count,
                minutes = weekSessions.Sum(s => s.DurationMinutes),
                calories = weekSessions.Sum(s => s.CaloriesBurned)
            });
        }
        return breakdown;
    }

    private object GetSessionTypeDistribution(List<TrainingSessionLog> sessions)
    {
        return sessions
            .GroupBy(s => s.SessionType)
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                percentage = (double)g.Count() / sessions.Count * 100
            })
            .ToList();
    }

    private List<object> GetPerformanceTrend(List<TrainingSessionLog> sessions, int weeks)
    {
        var trend = new List<object>();
        for (int i = weeks - 1; i >= 0; i--)
        {
            var weekStart = DateTime.UtcNow.AddDays(-i * 7);
            var weekEnd = weekStart.AddDays(7);
            var weekSessions = sessions.Where(s => s.Date >= weekStart && s.Date < weekEnd).ToList();

            trend.Add(new
            {
                week = weekStart.ToString("MMM dd"),
                averageDuration = weekSessions.Any() ? weekSessions.Average(s => s.DurationMinutes) : 0,
                averageCalories = weekSessions.Any() ? weekSessions.Average(s => s.CaloriesBurned) : 0,
                sessionCount = weekSessions.Count
            });
        }
        return trend;
    }

    private async Task<string> GetMonthlyWeightTrend(string userId)
    {
        var monthlyMeasurements = await _measurementRepository.GetByUserIdAsync(userId, "weight", DateTime.UtcNow.AddDays(-30));
        var ordered = monthlyMeasurements.OrderBy(m => m.Date).ToList();

        if (ordered.Count < 2) return "stable";

        var change = ordered.Last().Value - ordered.First().Value;
        return Math.Abs(change) < 0.5 ? "stable" : change > 0 ? "gaining" : "losing";
    }

    private int GetTargetSessions(string activityLevel)
    {
        return activityLevel.ToLower() switch
        {
            "sedentario" => 8,
            "leve" => 12,
            "moderado" => 16,
            "alto" => 20,
            _ => 12
        };
    }
}