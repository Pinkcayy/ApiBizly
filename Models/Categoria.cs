using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Categoria
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
