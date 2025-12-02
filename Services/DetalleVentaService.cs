using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class DetalleVentaService
{
    private readonly IMongoCollection<DetalleVenta> _collection;

    public DetalleVentaService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<DetalleVenta>("DetalleVentas");
    }

    public Task<List<DetalleVenta>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<DetalleVenta?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task<List<DetalleVenta>> GetByVentaIdAsync(string ventaId) =>
        _collection.Find(x => x.VentaId == ventaId).ToListAsync();

    public Task CreateAsync(DetalleVenta entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, DetalleVenta entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}

