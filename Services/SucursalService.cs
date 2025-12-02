using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class SucursalService
{
    private readonly IMongoCollection<Sucursal> _collection;

    public SucursalService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Sucursal>("Sucursales");
    }

    public Task<List<Sucursal>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Sucursal?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Sucursal entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Sucursal entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}

