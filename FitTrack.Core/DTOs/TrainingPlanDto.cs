namespace FitTrack.Core.DTOs;

public class TrainingPlanDto
{
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Goal { get; set; } = "perder";
    public string Focus { get; set; } = "geral";
    public int DurationWeeks { get; set; } = 4;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TrainingWeekDto> Weeks { get; set; } = new();
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class TrainingWeekDto
{
    public int WeekNumber { get; set; }
    public List<TrainingSessionDto> Sessions { get; set; } = new();
}

public class TrainingSessionDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string Type { get; set; } = "mixed";
    public int DurationMinutes { get; set; } = 30;
    public string Status { get; set; } = "planned";
    public List<SessionExerciseDto> Exercises { get; set; } = new();
}

public class SessionExerciseDto
{
    public required string ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; } = 3;
    public string Reps { get; set; } = "12";
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public int RestSeconds { get; set; } = 60;
}