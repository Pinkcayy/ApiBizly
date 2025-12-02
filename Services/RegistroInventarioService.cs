using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class RegistroInventarioService
{
    private readonly IMongoCollection<RegistroInventario> _collection;

    public RegistroInventarioService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<RegistroInventario>("Registros_inventario");
    }

    public Task<List<RegistroInventario>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<RegistroInventario?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task<List<RegistroInventario>> GetByInsumoIdAsync(string insumoId) =>
        _collection.Find(x => x.InsumoId == insumoId).ToListAsync();

    public Task CreateAsync(RegistroInventario entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, RegistroInventario entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
