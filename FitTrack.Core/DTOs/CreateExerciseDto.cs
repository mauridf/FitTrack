namespace FitTrack.Core.DTOs;

public class CreateExerciseDto
{
    public required string Name { get; set; }
    public string? BodyPart { get; set; }
    public string? TargetMuscle { get; set; }
    public List<string> Equipment { get; set; } = new();
    public List<string> Instructions { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public string? Difficulty { get; set; }
    public bool IsPublic { get; set; } = false; // Add this
}