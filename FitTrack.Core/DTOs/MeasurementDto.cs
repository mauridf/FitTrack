namespace FitTrack.Core.DTOs;

public class MeasurementDto
{
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public required string Type { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = "kg";
    public DateTime Date { get; set; }
}