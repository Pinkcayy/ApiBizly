using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class InsumoProductoVentaService
{
    private readonly IMongoCollection<InsumoProductoVenta> _collection;

    public InsumoProductoVentaService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<InsumoProductoVenta>("InsumoProductoVenta");
    }

    public Task<List<InsumoProductoVenta>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<InsumoProductoVenta?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task<List<InsumoProductoVenta>> GetByProductoVentaIdAsync(string productoVentaId) =>
        _collection.Find(x => x.ProductoVentaId == productoVentaId).ToListAsync();

    public Task CreateAsync(InsumoProductoVenta entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, InsumoProductoVenta entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}

