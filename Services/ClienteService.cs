using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class ClienteService
{
    private readonly IMongoCollection<Cliente> _collection;

    public ClienteService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Cliente>("Clientes");
    }

    public Task<List<Cliente>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Cliente?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Cliente entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Cliente entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
