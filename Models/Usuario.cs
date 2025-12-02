using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? SucursalId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? TrabajadorId { get; set; }

    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string TipoUsuario { get; set; } = null!; // "EMPRENDEDOR" / "TRABAJADOR"
    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
