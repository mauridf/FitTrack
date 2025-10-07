using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;

namespace FitTrack.Core.Interfaces;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(string id);
    Task<Exercise?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Exercise>> SearchAsync(ExerciseSearchDto searchDto);
    Task<IEnumerable<Exercise>> GetByUserIdAsync(string userId); // Add this
    Task<IEnumerable<Exercise>> GetPublicExercisesAsync(); // Add this
    Task<Exercise> CreateAsync(Exercise exercise);
    Task UpdateAsync(Exercise exercise);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExternalIdExistsAsync(string externalId);
    Task<IEnumerable<string>> GetBodyPartsAsync();
    Task<IEnumerable<string>> GetTargetMusclesAsync();
    Task<IEnumerable<string>> GetEquipmentListAsync();
}