using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class ProductoVentaService
{
    private readonly IMongoCollection<ProductoVenta> _collection;

    public ProductoVentaService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<ProductoVenta>("ProductosVenta");
    }

    public Task<List<ProductoVenta>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<ProductoVenta?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(ProductoVenta entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, ProductoVenta entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}

