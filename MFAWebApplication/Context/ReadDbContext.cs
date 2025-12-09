using MFAWebApplication.Entities;
using MongoDB.Driver;

namespace MFAWebApplication.Context;

public class ReadDbContext
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<ReadDbContext> _logger;

    public ReadDbContext(
        IConfiguration configuration,
        ILogger<ReadDbContext> logger)
    {
        _logger = logger;

        var connectionString = configuration.GetConnectionString("MongoDB_Read_Connection_String");
        var databaseName = configuration["MongoSettings:DatabaseName"];

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            _logger.LogError("MongoDB connection string or database name is missing in configuration: {connectionString}", connectionString);
        }

        var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);
        var client = new MongoClient(mongoSettings);
        _database = client.GetDatabase(databaseName);

        CreateIndexes();

    }

    private void CreateIndexes()
    {
        var users = _database.GetCollection<UserReadModel>("UserReadModel");

        var index = new CreateIndexModel<UserReadModel>(
            Builders<UserReadModel>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true }
        );

        users.Indexes.CreateOne(index);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

}