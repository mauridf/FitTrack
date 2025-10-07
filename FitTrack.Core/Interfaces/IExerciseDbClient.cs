using FitTrack.Core.DTOs;

namespace FitTrack.Core.Interfaces;

public interface IExerciseDbClient
{
    Task<List<ExerciseDto>> GetExercisesAsync();
    Task<List<ExerciseDto>> GetExercisesByBodyPartAsync(string bodyPart);
    Task<List<ExerciseDto>> GetExercisesByEquipmentAsync(string equipment);
    Task<List<ExerciseDto>> GetExercisesByTargetMuscleAsync(string target);
    Task<List<string>> GetBodyPartsAsync();
    Task<List<string>> GetEquipmentListAsync();
    Task<List<string>> GetTargetMusclesAsync();
}