namespace FitTrack.Core.DTOs;

public class TrainingSessionLogDto
{
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public string? PlanId { get; set; }
    public string? SessionId { get; set; }
    public DateTime Date { get; set; }
    public string SessionType { get; set; } = "mixed";
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public string? Notes { get; set; }
    public List<SessionExerciseLogDto> Exercises { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SessionExerciseLogDto
{
    public required string ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public List<ExerciseSetDto> Sets { get; set; } = new();
    public bool Completed { get; set; } = true;
}

public class ExerciseSetDto
{
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public bool Completed { get; set; } = true;
}

public class CreateSessionLogDto
{
    public string? PlanId { get; set; }
    public string? SessionId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string SessionType { get; set; } = "mixed";
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public string? Notes { get; set; }
    public List<CreateSessionExerciseLogDto> Exercises { get; set; } = new();
}

public class CreateSessionExerciseLogDto
{
    public required string ExerciseId { get; set; }
    public List<CreateExerciseSetDto> Sets { get; set; } = new();
    public bool Completed { get; set; } = true;
}

public class CreateExerciseSetDto
{
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public bool Completed { get; set; } = true;
}