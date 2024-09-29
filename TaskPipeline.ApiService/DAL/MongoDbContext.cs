using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using TaskPipeline.ApiService.Models;

namespace TaskPipeline.ApiService.DAL;

public class MongoDbContext : DbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("YourDatabaseName");
    }

    public IMongoCollection<Models.Task> Tasks => _database.GetCollection<Models.Task>("Tasks");
    public IMongoCollection<Pipeline> Pipelines => _database.GetCollection<Pipeline>("Pipelines");
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
}
