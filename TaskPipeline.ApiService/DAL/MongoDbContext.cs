using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using TaskPipeline.ApiService.Pipelines;
using TaskPipeline.ApiService.Tasks;
using TaskPipeline.ApiService.Users;

namespace TaskPipeline.ApiService.DAL;

public class MongoDbContext : DbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("YourDatabaseName");
    }

    public IMongoCollection<ExecutableTask> Tasks => _database.GetCollection<ExecutableTask>("Tasks");
    public IMongoCollection<Pipeline> Pipelines => _database.GetCollection<Pipeline>("Pipelines");
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
}
