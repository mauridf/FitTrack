namespace FitTrack.Core.DTOs;

public class ExerciseSearchDto
{
    public string? UserId { get; set; } // Add this - para filtrar por usuário
    public string? Name { get; set; }
    public string? BodyPart { get; set; }
    public string? TargetMuscle { get; set; }
    public string? Equipment { get; set; }
    public string? Difficulty { get; set; }
    public bool? OnlyPublic { get; set; } = true; // Add this - apenas exercícios públicos
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}