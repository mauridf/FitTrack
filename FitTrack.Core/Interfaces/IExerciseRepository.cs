using FitTrack.Core.DTOs;
using FitTrack.Core.Entities;

namespace FitTrack.Core.Interfaces;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(string id);
    Task<Exercise?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Exercise>> SearchAsync(ExerciseSearchDto searchDto);
    Task<Exercise> CreateAsync(Exercise exercise);
    Task UpdateAsync(Exercise exercise);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExternalIdExistsAsync(string externalId);
    Task<IEnumerable<string>> GetBodyPartsAsync();
    Task<IEnumerable<string>> GetTargetMusclesAsync();
    Task<IEnumerable<string>> GetEquipmentListAsync();
}