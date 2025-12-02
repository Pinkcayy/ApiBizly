using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Empresa
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Nombre { get; set; } = null!;
    public string Rubro { get; set; } = null!;
    public string Descripcion { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal MargenGanancia { get; set; }

    public string LogoUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
