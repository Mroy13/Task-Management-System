using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<TaskManagementDBSettings> Options)
        {

            var connectionString = Options.Value.ConnectionString;
            var databaseName = Options.Value.DatabaseName;
            var mongoClient = new MongoClient(connectionString);
            _database = mongoClient.GetDatabase(databaseName);
        }
        public IMongoCollection<T> GetCollection<T>(string? collectionName = null)
        {

            var name = collectionName ?? typeof(T).Name + "s";
            return _database.GetCollection<T>(name);
        }
    }
}
