using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Venta
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string SucursalId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ClienteId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public string MetodoPago { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Total { get; set; }

    public bool EsEnvio { get; set; }

    public string EstadoPago { get; set; } = null!; // "pagado" / "pendiente"

    public string EstadoPedido { get; set; } = null!; // "pendiente" / "completado" / "cancelado"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
