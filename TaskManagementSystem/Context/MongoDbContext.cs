using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Context;

/// <summary>
/// Provides MongoDB database access and typed collections (default collection name: type name + "s").
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<TaskManagementDBSettings> options)
    {
        var connectionString = options.Value.ConnectionString;
        var databaseName = options.Value.DatabaseName;
        var mongoClient = new MongoClient(connectionString);
        _database = mongoClient.GetDatabase(databaseName);
    }

    /// <param name="collectionName">Optional override; defaults to <c>{TypeName}s</c>.</param>
    public IMongoCollection<T> GetCollection<T>(string collectionName = null)
    {
        var name = collectionName ?? typeof(T).Name + "s";
        return _database.GetCollection<T>(name);
    }
}
