using System.Text;
using ApiBizly.Models;
using ApiBizly.Services;
using ApiBizly.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// =====================
//  CONFIGURACIÓN MONGO
// =====================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;

    if (string.IsNullOrEmpty(settings.ConnectionString))
    {
        throw new Exception("MongoDbSettings.ConnectionString no está configurado. Verifica tu appsettings o variables de entorno.");
    }

    return settings;
});

// =====================
//  REGISTRO DE SERVICES
// =====================
builder.Services.AddSingleton<UsuarioService>();
builder.Services.AddSingleton<VentaService>();
builder.Services.AddSingleton<CostoGastoService>();
builder.Services.AddSingleton<InsumoService>();
builder.Services.AddSingleton<ClienteService>();
builder.Services.AddSingleton<EmpresaService>();
builder.Services.AddSingleton<CategoriaService>();
builder.Services.AddSingleton<RegistroInventarioService>();
builder.Services.AddSingleton<SucursalService>();
builder.Services.AddSingleton<TrabajadorService>();
builder.Services.AddSingleton<ProductoVentaService>();
builder.Services.AddSingleton<DetalleVentaService>();
builder.Services.AddSingleton<InsumoProductoVentaService>();

// =====================
//  CONFIGURACIÓN JWT
// =====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings is null)
    throw new Exception("JwtSettings no está configurado en appsettings.json");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmprendedorOnly", policy => policy.RequireRole("EMPRENDEDOR"));
    options.AddPolicy("TrabajadorOnly", policy => policy.RequireRole("TRABAJADOR"));
});

// =====================
//  API REST (Controllers)
// =====================
builder.Services.AddControllers();

// =====================
//  CORS (para que el front pueda consumir la API)
// =====================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =====================
//  Swagger
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================
//  GRAPHQL
// =====================
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))

    // CATEGORÍA
    .AddTypeExtension<CategoriaQueries>()
    .AddTypeExtension<CategoriaMutations>()

    // CLIENTE
    .AddTypeExtension<ClienteQueries>()
    .AddTypeExtension<ClienteMutations>()

    // USUARIO
    .AddTypeExtension<UsuarioQueries>()
    .AddTypeExtension<UsuarioMutations>();

// =====================
//  BUILD
// =====================
var app = builder.Build();

// =====================
//  Swagger SIEMPRE (para pruebas en producción)
// =====================
app.UseSwagger();
app.UseSwaggerUI();

// =====================
//  Middleware
// =====================
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// =====================
//  MAP CONTROLLERS + GRAPHQL
// =====================
try
{
    // REST API
    app.MapControllers();

    // GraphQL endpoint
    app.MapGraphQL("/graphql");
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al cargar tipos:");
    if (ex.LoaderExceptions != null)
    {
        foreach (var loaderEx in ex.LoaderExceptions)
        {
            logger.LogError("LoaderException: {Message}", loaderEx?.Message);
        }
    }
    throw;
}

// =====================
//  PUERTO PARA RAILWAY
// =====================
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();
