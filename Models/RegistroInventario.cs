using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class RegistroInventario
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
    public string InsumoId { get; set; } = null!;

    public string TipoMovimiento { get; set; } = null!; // "entrada" / "salida" / "ajuste"

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CantidadAnterior { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CantidadNueva { get; set; }

    public string Motivo { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
