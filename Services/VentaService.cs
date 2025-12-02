using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class VentaService
{
    private readonly IMongoCollection<Venta> _collection;

    public VentaService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Venta>("Ventas");
    }

    public Task<List<Venta>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Venta?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Venta entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Venta entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
