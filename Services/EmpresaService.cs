using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class EmpresaService
{
    private readonly IMongoCollection<Empresa> _collection;

    public EmpresaService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Empresa>("Empresas");
    }

    public Task<List<Empresa>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Empresa?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Empresa entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Empresa entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);
}
