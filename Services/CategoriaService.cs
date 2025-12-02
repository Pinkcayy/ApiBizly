using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class CategoriaService
{
    private readonly IMongoCollection<Categoria> _collection;

    public CategoriaService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Categoria>("Categorias");
    }

    public Task<List<Categoria>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Categoria?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Categoria entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Categoria entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
