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
}