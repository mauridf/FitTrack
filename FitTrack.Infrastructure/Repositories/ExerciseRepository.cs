using MongoDB.Driver;
using FitTrack.Core.Entities;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;
using FitTrack.Infrastructure.Data;

namespace FitTrack.Infrastructure.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly MongoDbContext _context;

    public ExerciseRepository(MongoDbContext context)
    {
        _context = context;
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Índice composto para UserId + outros campos
        var userIndexKeys = Builders<Exercise>.IndexKeys
            .Ascending(e => e.UserId)
            .Ascending(e => e.IsPublic)
            .Ascending(e => e.BodyPart)
            .Ascending(e => e.TargetMuscle);

        var userIndexOptions = new CreateIndexOptions { Name = "UserId_SearchIndex" };
        var userIndexModel = new CreateIndexModel<Exercise>(userIndexKeys, userIndexOptions);

        _context.Exercises.Indexes.CreateOne(userIndexModel);

        // Índice de texto apenas para Name
        var textIndexKeys = Builders<Exercise>.IndexKeys.Text(e => e.Name);
        var textIndexOptions = new CreateIndexOptions { Name = "NameTextIndex" };
        var textIndexModel = new CreateIndexModel<Exercise>(textIndexKeys, textIndexOptions);

        _context.Exercises.Indexes.CreateOne(textIndexModel);

        // Índice para Equipment (campo array)
        var equipmentIndexKeys = Builders<Exercise>.IndexKeys.Ascending(e => e.Equipment);
        var equipmentIndexOptions = new CreateIndexOptions { Name = "EquipmentIndex" };
        var equipmentIndexModel = new CreateIndexModel<Exercise>(equipmentIndexKeys, equipmentIndexOptions);

        _context.Exercises.Indexes.CreateOne(equipmentIndexModel);

        // Índice único para ExternalId
        var externalIdIndex = Builders<Exercise>.IndexKeys.Ascending(e => e.ExternalId);
        var externalIdIndexModel = new CreateIndexModel<Exercise>(externalIdIndex, new CreateIndexOptions
        {
            Name = "ExternalIdIndex",
            Unique = true,
            Sparse = true
        });

        _context.Exercises.Indexes.CreateOne(externalIdIndexModel);
    }

    public async Task<Exercise?> GetByIdAsync(string id)
    {
        return await _context.Exercises.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Exercise?> GetByExternalIdAsync(string externalId)
    {
        return await _context.Exercises.Find(e => e.ExternalId == externalId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Exercise>> SearchAsync(ExerciseSearchDto searchDto)
    {
        var filterBuilder = Builders<Exercise>.Filter;

        // Filtro base: exercícios públicos OU do usuário específico
        var baseFilter = filterBuilder.Or(
            filterBuilder.Eq(e => e.IsPublic, true),
            filterBuilder.Eq(e => e.UserId, searchDto.UserId)
        );

        var filter = baseFilter;

        if (!string.IsNullOrEmpty(searchDto.Name))
        {
            filter &= filterBuilder.Text(searchDto.Name, new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false });
        }

        if (!string.IsNullOrEmpty(searchDto.BodyPart))
        {
            filter &= filterBuilder.Eq(e => e.BodyPart, searchDto.BodyPart);
        }

        if (!string.IsNullOrEmpty(searchDto.TargetMuscle))
        {
            filter &= filterBuilder.Eq(e => e.TargetMuscle, searchDto.TargetMuscle);
        }

        if (!string.IsNullOrEmpty(searchDto.Equipment))
        {
            filter &= filterBuilder.AnyEq(e => e.Equipment, searchDto.Equipment);
        }

        if (!string.IsNullOrEmpty(searchDto.Difficulty))
        {
            filter &= filterBuilder.Eq(e => e.Difficulty, searchDto.Difficulty);
        }

        // Se userId não foi fornecido, mostrar apenas públicos
        if (string.IsNullOrEmpty(searchDto.UserId))
        {
            filter = filterBuilder.Eq(e => e.IsPublic, true);

            // Aplicar outros filtros
            if (!string.IsNullOrEmpty(searchDto.Name))
                filter &= filterBuilder.Text(searchDto.Name);
            if (!string.IsNullOrEmpty(searchDto.BodyPart))
                filter &= filterBuilder.Eq(e => e.BodyPart, searchDto.BodyPart);
            // ... outros filtros
        }

        var skip = (searchDto.Page - 1) * searchDto.PageSize;

        return await _context.Exercises
            .Find(filter)
            .SortBy(e => e.Name)
            .Skip(skip)
            .Limit(searchDto.PageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> GetByUserIdAsync(string userId)
    {
        return await _context.Exercises
            .Find(e => e.UserId == userId)
            .SortBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> GetPublicExercisesAsync()
    {
        return await _context.Exercises
            .Find(e => e.IsPublic == true)
            .SortBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Exercise> CreateAsync(Exercise exercise)
    {
        await _context.Exercises.InsertOneAsync(exercise);
        return exercise;
    }

    public async Task UpdateAsync(Exercise exercise)
    {
        exercise.UpdatedAt = DateTime.UtcNow;
        await _context.Exercises.ReplaceOneAsync(e => e.Id == exercise.Id, exercise);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Exercises.DeleteOneAsync(e => e.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExternalIdExistsAsync(string externalId)
    {
        return await _context.Exercises.Find(e => e.ExternalId == externalId).AnyAsync();
    }

    public async Task<IEnumerable<string>> GetBodyPartsAsync()
    {
        return await _context.Exercises
            .Distinct(e => e.BodyPart, Builders<Exercise>.Filter.Empty)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetTargetMusclesAsync()
    {
        return await _context.Exercises
            .Distinct(e => e.TargetMuscle, Builders<Exercise>.Filter.Empty)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetEquipmentListAsync()
    {
        var allExercises = await _context.Exercises.Find(_ => true).ToListAsync();
        return allExercises
            .SelectMany(e => e.Equipment)
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }
}