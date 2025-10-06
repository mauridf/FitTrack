using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitTrack.Core.Entities;

public class Measurement
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; }

    public required string Type { get; set; } // weight, waist, chest, etc.
    public double Value { get; set; }
    public string Unit { get; set; } = "kg";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}