using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitTrack.Core.Entities;

public class Exercise
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? ExternalId { get; set; } // ID da API externa
    public required string Name { get; set; }
    public string? BodyPart { get; set; }
    public string? TargetMuscle { get; set; }
    public List<string> Equipment { get; set; } = new();
    public string? GifUrl { get; set; }
    public List<string> Instructions { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public string? Difficulty { get; set; }
    public bool IsCustom { get; set; } = false; // Se foi criado pelo usuário

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}