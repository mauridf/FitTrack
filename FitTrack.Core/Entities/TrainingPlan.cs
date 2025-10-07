using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitTrack.Core.Entities;

public class TrainingPlan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Goal { get; set; } = "perder"; // perder/manter/ganhar
    public string Focus { get; set; } = "geral"; // cardio/força/hiit
    public int DurationWeeks { get; set; } = 4;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<TrainingWeek> Weeks { get; set; } = new();
    public string Status { get; set; } = "active"; // active/completed/cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TrainingWeek
{
    public int WeekNumber { get; set; }
    public List<TrainingSession> Sessions { get; set; } = new();
}

public class TrainingSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string Type { get; set; } = "mixed"; // cardio/strength/hiit
    public int DurationMinutes { get; set; } = 30;
    public string Status { get; set; } = "planned"; // planned/completed/skipped

    public List<SessionExercise> Exercises { get; set; } = new();
}

public class SessionExercise
{
    [BsonRepresentation(BsonType.ObjectId)]
    public required string ExerciseId { get; set; }

    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; } = 3;
    public string Reps { get; set; } = "12"; // pode ser "12" ou "30s" para duração
    public double? WeightKg { get; set; }
    public int? DurationSeconds { get; set; } // para exercícios de duração
    public int RestSeconds { get; set; } = 60;
}