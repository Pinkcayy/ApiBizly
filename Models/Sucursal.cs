using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiBizly.Models;

public class Sucursal
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string EmpresaId { get; set; } = null!;

    public string Nombre { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Ciudad { get; set; } = null!;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Latitud { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Longitud { get; set; }

    public string Departamento { get; set; } = null!;
    public string Telefono { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

