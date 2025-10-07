namespace FitTrack.Core.DTOs;

public class CreateTrainingPlanDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Goal { get; set; } = "perder";
    public string Focus { get; set; } = "geral";
    public int DurationWeeks { get; set; } = 4;
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
}