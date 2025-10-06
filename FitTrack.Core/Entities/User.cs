using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitTrack.Core.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public DateTime? BirthDate { get; set; }
    public char? Gender { get; set; } // M/F
    public int HeightCm { get; set; }
    public double WeightKg { get; set; }

    public string ActivityLevel { get; set; } = "sedentario"; // sedentario/leve/moderado/alto
    public string Goal { get; set; } = "manter"; // perder/manter/ganhar
    public double TargetWeightKg { get; set; }

    public List<string> AvailableEquipment { get; set; } = new(); // esteira, halteres, etc.
    public List<string> Restrictions { get; set; } = new();
    public string Preference { get; set; } = "casa"; // casa/academia

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}