using FitTrack.Core.Interfaces;

namespace FitTrack.Infrastructure.Services;

public class CalculationService : ICalculationService
{
    public double CalculateBMI(double weightKg, double heightCm)
    {
        var heightMeters = heightCm / 100.0;
        var bmi = weightKg / (heightMeters * heightMeters);
        return Math.Round(bmi, 2);
    }

    public string ClassifyBMI(double bmi)
    {
        return bmi switch
        {
            < 18.5 => "Abaixo do peso",
            >= 18.5 and < 25 => "Peso normal",
            >= 25 and < 30 => "Sobrepeso",
            _ => "Obesidade"
        };
    }

    public double CalculateIdealWeight(double heightCm, char? gender)
    {
        if (gender == 'M')
        {
            // Homens: pesoIdeal = alturaCm - 100 - ((alturaCm - 150)/4)
            return Math.Round(heightCm - 100 - ((heightCm - 150) / 4.0), 1);
        }
        else
        {
            // Mulheres: pesoIdeal = alturaCm - 100 - ((alturaCm - 150)/2.5)
            return Math.Round(heightCm - 100 - ((heightCm - 150) / 2.5), 1);
        }
    }
}