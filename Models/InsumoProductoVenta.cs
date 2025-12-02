using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class InsumoProductoVenta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductoVentaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string InsumoId { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CantidadUtilizada { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

