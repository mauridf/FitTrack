using Microsoft.Extensions.Options;
using MongoDB.Driver;
using FitTrack.Core.Entities;

namespace FitTrack.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    // Coleções (equivalente a tabelas no SQL)
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Measurement> Measurements => _database.GetCollection<Measurement>("measurements");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refresh_tokens");
    public IMongoCollection<Exercise> Exercises => _database.GetCollection<Exercise>("exercises");
    public IMongoCollection<TrainingPlan> TrainingPlans => _database.GetCollection<TrainingPlan>("training_plans");
    public IMongoCollection<TrainingSessionLog> TrainingSessions => _database.GetCollection<TrainingSessionLog>("training_sessions");
}