namespace FitTrack.Core.DTOs;

public class ExerciseSearchDto
{
    public string? Name { get; set; }
    public string? BodyPart { get; set; }
    public string? TargetMuscle { get; set; }
    public string? Equipment { get; set; }
    public string? Difficulty { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}