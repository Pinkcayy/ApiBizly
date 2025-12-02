using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Cliente
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? SucursalId { get; set; }

    public string Nombre { get; set; } = null!;
    public int Nit { get; set; }
    public string Telefono { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Direccion { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
