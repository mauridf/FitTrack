namespace FitTrack.Core.DTOs;

public class CreateMeasurementDto
{
    public required string Type { get; set; } // weight, waist, chest, etc.
    public double Value { get; set; }
    public string Unit { get; set; } = "kg";
    public DateTime? Date { get; set; }
}