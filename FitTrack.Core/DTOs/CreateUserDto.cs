namespace FitTrack.Core.DTOs;

public class CreateUserDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTime? BirthDate { get; set; }
    public char? Gender { get; set; }
    public int HeightCm { get; set; }
    public double WeightKg { get; set; }
    public string ActivityLevel { get; set; } = "sedentario";
    public string Goal { get; set; } = "manter";
    public double TargetWeightKg { get; set; }
    public List<string> AvailableEquipment { get; set; } = new();
    public List<string> Restrictions { get; set; } = new();
    public string Preference { get; set; } = "casa";
}