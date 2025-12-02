using ApiBizly.Models;
using MongoDB.Driver;

namespace ApiBizly.Services;

public class UsuarioService
{
    private readonly IMongoCollection<Usuario> _collection;

    public UsuarioService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.DatabaseName);
        _collection = db.GetCollection<Usuario>("Usuarios");
    }

   
    public Task<List<Usuario>> GetAllAsync() =>
        _collection.Find(_ => true).ToListAsync();

    public Task<Usuario?> GetByIdAsync(string id) =>
        _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task CreateAsync(Usuario entity) =>
        _collection.InsertOneAsync(entity);

    public Task UpdateAsync(string id, Usuario entity) =>
        _collection.ReplaceOneAsync(x => x.Id == id, entity);

    public Task DeleteAsync(string id) =>
        _collection.DeleteOneAsync(x => x.Id == id);


    public async Task<Usuario?> GetByEmailAndPasswordAsync(string email, string contrasena)
    {
        return await _collection
            .Find(u =>
                u.Email == email &&
                u.Contrasena == contrasena &&
                u.Estado == true
            )
            .FirstOrDefaultAsync();
    }
}
