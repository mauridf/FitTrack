namespace FitTrack.Core.Interfaces;

public interface ICalculationService
{
    double CalculateBMI(double weightKg, double heightCm);
    string ClassifyBMI(double bmi);
    double CalculateIdealWeight(double heightCm, char? gender);
}