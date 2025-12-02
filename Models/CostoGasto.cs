using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class CostoGasto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? SucursalId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = null!;

    public string CategoriaFinanciera { get; set; } = null!; // "DIRECTO" / "ADMINISTRATIVO"

    public string Descripcion { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public string Clasificacion { get; set; } = null!; // "FIJO" / "VARIABLE"

    [BsonRepresentation(BsonType.ObjectId)]
    public string? InsumoId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? TrabajadorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
