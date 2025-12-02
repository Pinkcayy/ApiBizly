using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class CostoGastoService
{
    private readonly IMongoCollection<CostoGasto> _collection;

    public CostoGastoService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<CostoGasto>("Costos_gastos");
    }

    public Task<List<CostoGasto>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<CostoGasto?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(CostoGasto entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, CostoGasto entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
