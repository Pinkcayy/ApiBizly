using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Insumo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string SucursalId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Cantidad { get; set; }

    public string UnidadMedida { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PrecioUnitario { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PrecioTotal { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal StockMinimo { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
