using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class InsumoService
{
    private readonly IMongoCollection<Insumo> _collection;

    public InsumoService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Insumo>("Insumos");
    }

    public Task<List<Insumo>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Insumo?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Insumo entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Insumo entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
