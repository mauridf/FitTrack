namespace FitTrack.Core.DTOs;

public class ExerciseDto
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public string ExternalId { get; set; } = string.Empty; // Mude para não-nullable
    public required string Name { get; set; }
    public string? BodyPart { get; set; }
    public string? TargetMuscle { get; set; }
    public List<string> Equipment { get; set; } = new();
    public string? GifUrl { get; set; }
    public List<string> Instructions { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public string? Difficulty { get; set; }
    public bool IsCustom { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
}