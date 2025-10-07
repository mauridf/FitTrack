using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitTrack.Core.Entities;

public class Exercise
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    public string ExternalId { get; set; } = GenerateCustomExternalId(); // Mude para não-nullable com valor default
    public required string Name { get; set; }
    public string? BodyPart { get; set; }
    public string? TargetMuscle { get; set; }
    public List<string> Equipment { get; set; } = new();
    public string? GifUrl { get; set; }
    public List<string> Instructions { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public string? Difficulty { get; set; }
    public bool IsCustom { get; set; } = false;
    public bool IsPublic { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Método para gerar ExternalId único para exercícios customizados
    private static string GenerateCustomExternalId()
    {
        return $"custom_{Guid.NewGuid():N}"; // Exemplo: "custom_a1b2c3d4e5f6"
    }
}