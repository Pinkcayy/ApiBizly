using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class DetalleVenta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string VentaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductoVentaId { get; set; } = null!;

    public int Cantidad { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PrecioUnitario { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Subtotal { get; set; }
}
