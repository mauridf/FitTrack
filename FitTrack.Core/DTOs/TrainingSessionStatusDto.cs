namespace FitTrack.Core.DTOs;

public class TrainingSessionStatusDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    // Informações do plano
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
}