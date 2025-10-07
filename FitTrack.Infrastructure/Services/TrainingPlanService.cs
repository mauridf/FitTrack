using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;

namespace FitTrack.Infrastructure.Services;

public class TrainingPlanService : ITrainingPlanService
{
    private readonly IUserRepository _userRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ITrainingPlanRepository _planRepository;

    public TrainingPlanService(
        IUserRepository userRepository,
        IExerciseRepository exerciseRepository,
        ITrainingPlanRepository planRepository)
    {
        _userRepository = userRepository;
        _exerciseRepository = exerciseRepository;
        _planRepository = planRepository;
    }

    public async Task<TrainingPlan> GenerateTrainingPlanAsync(string userId, CreateTrainingPlanDto createDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("Usuário não encontrado");
        }

        var plan = new TrainingPlan
        {
            UserId = userId,
            Name = createDto.Name,
            Description = createDto.Description,
            Goal = createDto.Goal,
            Focus = createDto.Focus,
            DurationWeeks = createDto.DurationWeeks,
            StartDate = createDto.StartDate,
            EndDate = createDto.StartDate.AddDays(createDto.DurationWeeks * 7),
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Gerar semanas do plano
        await GenerateWeeksAsync(plan, user);

        return await _planRepository.CreateAsync(plan);
    }

    private async Task GenerateWeeksAsync(TrainingPlan plan, User user)
    {
        var availableExercises = await GetAvailableExercisesAsync(user);

        for (int week = 1; week <= plan.DurationWeeks; week++)
        {
            var trainingWeek = new TrainingWeek { WeekNumber = week };

            // Definir sessões baseadas no nível de atividade
            var sessionsPerWeek = GetSessionsPerWeek(user.ActivityLevel);

            for (int day = 0; day < sessionsPerWeek; day++)
            {
                var sessionDate = plan.StartDate.AddDays((week - 1) * 7 + day);
                var session = await GenerateSessionAsync(sessionDate, plan.Focus, user, availableExercises, week);
                trainingWeek.Sessions.Add(session);
            }

            plan.Weeks.Add(trainingWeek);
        }
    }

    private async Task<TrainingSession> GenerateSessionAsync(DateTime date, string focus, User user,
        List<Exercise> availableExercises, int weekNumber)
    {
        var session = new TrainingSession
        {
            Date = date,
            DayOfWeek = date.DayOfWeek.ToString(),
            Type = GetSessionType(focus, weekNumber),
            DurationMinutes = GetSessionDuration(user.ActivityLevel, weekNumber),
            Status = "planned"
        };

        // Selecionar exercícios baseados no tipo de sessão e equipamentos disponíveis
        var sessionExercises = await SelectExercisesForSessionAsync(session.Type, user, availableExercises);

        foreach (var exercise in sessionExercises)
        {
            var sessionExercise = new SessionExercise
            {
                ExerciseId = exercise.Id!,
                ExerciseName = exercise.Name,
                Sets = GetSetsCount(user.ActivityLevel, weekNumber),
                Reps = GetRepsRange(user.ActivityLevel, exercise, session.Type),
                RestSeconds = GetRestTime(user.ActivityLevel)
            };

            session.Exercises.Add(sessionExercise);
        }

        return session;
    }

    private async Task<List<Exercise>> GetAvailableExercisesAsync(User user)
    {
        var searchDto = new ExerciseSearchDto
        {
            OnlyPublic = true // Incluir exercícios públicos
        };

        if (user.AvailableEquipment.Any())
        {
            // Buscar exercícios que usam equipamentos disponíveis OU não precisam de equipamento
            var exercises = await _exerciseRepository.SearchAsync(searchDto);
            return exercises
                .Where(e => !e.Equipment.Any() || e.Equipment.Any(eq => user.AvailableEquipment.Contains(eq)))
                .ToList();
        }

        // Se não tem equipamentos, retornar exercícios sem equipamento
        return (await _exerciseRepository.SearchAsync(searchDto))
            .Where(e => !e.Equipment.Any())
            .ToList();
    }

    private async Task<List<Exercise>> SelectExercisesForSessionAsync(string sessionType, User user, List<Exercise> availableExercises)
    {
        var selectedExercises = new List<Exercise>();

        switch (sessionType.ToLower())
        {
            case "cardio":
                selectedExercises = availableExercises
                    .Where(e => e.BodyPart?.ToLower() == "cardio")
                    .Take(3)
                    .ToList();
                break;

            case "strength":
                selectedExercises = availableExercises
                    .Where(e => e.BodyPart?.ToLower() != "cardio")
                    .Take(4)
                    .ToList();
                break;

            case "hiit":
                var cardio = availableExercises.Where(e => e.BodyPart?.ToLower() == "cardio").Take(2);
                var strength = availableExercises.Where(e => e.BodyPart?.ToLower() != "cardio").Take(3);
                selectedExercises = cardio.Concat(strength).ToList();
                break;

            default: // mixed
                selectedExercises = availableExercises.Take(5).ToList();
                break;
        }

        // Se não encontrou exercícios suficientes, completar com quaisquer disponíveis
        if (selectedExercises.Count < 3)
        {
            var extraExercises = availableExercises
                .Except(selectedExercises)
                .Take(3 - selectedExercises.Count);
            selectedExercises.AddRange(extraExercises);
        }

        return selectedExercises;
    }

    // Métodos auxiliares para configuração do treino
    private int GetSessionsPerWeek(string activityLevel)
    {
        return activityLevel.ToLower() switch
        {
            "sedentario" => 2,
            "leve" => 3,
            "moderado" => 4,
            "alto" => 5,
            _ => 3
        };
    }

    private int GetSessionDuration(string activityLevel, int weekNumber)
    {
        var baseDuration = activityLevel.ToLower() switch
        {
            "sedentario" => 20,
            "leve" => 30,
            "moderado" => 40,
            "alto" => 50,
            _ => 30
        };

        // Aumentar duração progressivamente
        return baseDuration + (weekNumber - 1) * 5;
    }

    private string GetSessionType(string focus, int weekNumber)
    {
        if (focus.ToLower() == "cardio") return "cardio";
        if (focus.ToLower() == "força") return "strength";
        if (focus.ToLower() == "hiit") return "hiit";

        // Para foco geral, alternar tipos
        var types = new[] { "mixed", "cardio", "strength", "hiit" };
        return types[weekNumber % types.Length];
    }

    private int GetSetsCount(string activityLevel, int weekNumber)
    {
        var baseSets = activityLevel.ToLower() switch
        {
            "sedentario" => 2,
            "leve" => 3,
            "moderado" => 3,
            "alto" => 4,
            _ => 3
        };

        return Math.Min(baseSets + (weekNumber / 2), 5); // Máximo 5 sets
    }

    private string GetRepsRange(string activityLevel, Exercise exercise, string sessionType)
    {
        if (sessionType == "cardio" || exercise.BodyPart?.ToLower() == "cardio")
        {
            return activityLevel.ToLower() switch
            {
                "sedentario" => "10-15min",
                "leve" => "15-20min",
                "moderado" => "20-25min",
                "alto" => "25-30min",
                _ => "15-20min"
            };
        }

        return activityLevel.ToLower() switch
        {
            "sedentario" => "8-12",
            "leve" => "10-15",
            "moderado" => "12-15",
            "alto" => "15-20",
            _ => "10-15"
        };
    }

    private int GetRestTime(string activityLevel)
    {
        return activityLevel.ToLower() switch
        {
            "sedentario" => 90,
            "leve" => 75,
            "moderado" => 60,
            "alto" => 45,
            _ => 60
        };
    }

    public async Task<TrainingPlan> RegenerateWeekAsync(TrainingPlan plan, int weekNumber)
    {
        var user = await _userRepository.GetByIdAsync(plan.UserId);
        if (user == null) return plan;

        var availableExercises = await GetAvailableExercisesAsync(user);
        var week = plan.Weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);

        if (week != null)
        {
            foreach (var session in week.Sessions)
            {
                var newExercises = await SelectExercisesForSessionAsync(session.Type, user, availableExercises);
                session.Exercises.Clear();

                foreach (var exercise in newExercises)
                {
                    session.Exercises.Add(new SessionExercise
                    {
                        ExerciseId = exercise.Id!,
                        ExerciseName = exercise.Name,
                        Sets = GetSetsCount(user.ActivityLevel, weekNumber),
                        Reps = GetRepsRange(user.ActivityLevel, exercise, session.Type),
                        RestSeconds = GetRestTime(user.ActivityLevel)
                    });
                }
            }
        }

        plan.UpdatedAt = DateTime.UtcNow;
        await _planRepository.UpdateAsync(plan);
        return plan;
    }

    public double CalculateProgress(TrainingPlan plan)
    {
        var totalSessions = plan.Weeks.Sum(w => w.Sessions.Count);
        var completedSessions = plan.Weeks.Sum(w => w.Sessions.Count(s => s.Status == "completed"));

        return totalSessions > 0 ? (double)completedSessions / totalSessions * 100 : 0;
    }

    public int CalculateCaloriesBurned(User user, TrainingSessionLog session)
    {
        // Fórmula simplificada para cálculo de calorias
        var baseCalories = user.WeightKg * 0.5;
        var durationFactor = session.DurationMinutes / 60.0;
        var intensityFactor = session.SessionType.ToLower() switch
        {
            "cardio" => 1.2,
            "strength" => 1.0,
            "hiit" => 1.5,
            _ => 1.1
        };

        return (int)(baseCalories * durationFactor * intensityFactor);
    }
}