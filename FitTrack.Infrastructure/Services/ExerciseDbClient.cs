using System.Text.Json;
using FitTrack.Core.DTOs;
using FitTrack.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FitTrack.Infrastructure.Services;

public class ExerciseDbClient : IExerciseDbClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExerciseDbClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        // Configurar base URL da ExerciseDB API
        _httpClient.BaseAddress = new Uri("https://exercisedb.p.rapidapi.com/");
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _configuration["ExerciseDb:ApiKey"] ?? "");
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", "exercisedb.p.rapidapi.com");
    }

    public async Task<List<ExerciseDto>> GetExercisesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("exercises?limit=1000");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return exercises ?? new List<ExerciseDto>();
        }
        catch (Exception ex)
        {
            // Log error (implementar logging depois)
            Console.WriteLine($"Error fetching exercises: {ex.Message}");
            return new List<ExerciseDto>();
        }
    }

    public async Task<List<ExerciseDto>> GetExercisesByBodyPartAsync(string bodyPart)
    {
        try
        {
            var response = await _httpClient.GetAsync($"exercises/bodyPart/{bodyPart}?limit=100");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return exercises ?? new List<ExerciseDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching exercises by body part: {ex.Message}");
            return new List<ExerciseDto>();
        }
    }

    public async Task<List<ExerciseDto>> GetExercisesByEquipmentAsync(string equipment)
    {
        try
        {
            var response = await _httpClient.GetAsync($"exercises/equipment/{equipment}?limit=100");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return exercises ?? new List<ExerciseDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching exercises by equipment: {ex.Message}");
            return new List<ExerciseDto>();
        }
    }

    public async Task<List<ExerciseDto>> GetExercisesByTargetMuscleAsync(string target)
    {
        try
        {
            var response = await _httpClient.GetAsync($"exercises/target/{target}?limit=100");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return exercises ?? new List<ExerciseDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching exercises by target muscle: {ex.Message}");
            return new List<ExerciseDto>();
        }
    }

    public async Task<List<string>> GetBodyPartsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("exercises/bodyPartList");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching body parts: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetEquipmentListAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("exercises/equipmentList");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching equipment list: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetTargetMusclesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("exercises/targetList");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching target muscles: {ex.Message}");
            return new List<string>();
        }
    }
}