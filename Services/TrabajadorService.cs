using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class TrabajadorService
{
    private readonly IMongoCollection<Trabajador> _collection;

    public TrabajadorService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Trabajador>("Trabajadores");
    }

    public Task<List<Trabajador>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Trabajador?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Trabajador entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Trabajador entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}

