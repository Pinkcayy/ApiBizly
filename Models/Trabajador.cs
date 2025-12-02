using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Trabajador
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? SucursalId { get; set; }

    public string Nombre { get; set; } = null!;
    public string Cargo { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SueldoMensual { get; set; }

    public string TipoGasto { get; set; } = null!; // "fijo" / "variable"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
