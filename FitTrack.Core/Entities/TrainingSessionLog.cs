using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitTrack.Core.Entities;

public class TrainingSessionLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? PlanId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? SessionId { get; set; } // ID da sessão no plano

    public DateTime Date { get; set; }
    public string SessionType { get; set; } = "mixed";
    public int DurationMinutes { get; set; }
    public int CaloriesBurned { get; set; }
    public string? Notes { get; set; }

    public List<SessionExerciseLog> Exercises { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SessionExerciseLog
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string ExerciseId { get; set; }

    public string ExerciseName { get; set; } = string.Empty;
    public List<ExerciseSet> Sets { get; set; } = new();
    public bool Completed { get; set; } = true;
}

public class ExerciseSet
{
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public bool Completed { get; set; } = true;
}