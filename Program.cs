using System.Text;
using ApiBizly.Models;
using ApiBizly.Services;
using ApiBizly.GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================
//  CONFIGURACIÓN MONGO
// =====================
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;

    // En producción, verificar variables de entorno primero
    var connectionString = Environment.GetEnvironmentVariable("MongoDbSettings__ConnectionString") 
                           ?? settings.ConnectionString;
    
    var databaseName = Environment.GetEnvironmentVariable("MongoDbSettings__DatabaseName") 
                       ?? settings.DatabaseName;

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("MongoDbSettings.ConnectionString no está configurado. Verifica tu appsettings o variables de entorno.");
    }

    // Actualizar con valores de entorno si existen
    settings.ConnectionString = connectionString;
    if (!string.IsNullOrEmpty(databaseName))
    {
        settings.DatabaseName = databaseName;
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
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

// Priorizar variables de entorno sobre appsettings.json
jwtSettings.Key = Environment.GetEnvironmentVariable("JwtSettings__Key") ?? jwtSettings.Key ?? string.Empty;
jwtSettings.Issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") ?? jwtSettings.Issuer ?? "ApiBizly";
jwtSettings.Audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") ?? jwtSettings.Audience ?? "ApiBizlyClients";
if (int.TryParse(Environment.GetEnvironmentVariable("JwtSettings__ExpirationMinutes"), out int expiration))
{
    jwtSettings.ExpirationMinutes = expiration;
}
else if (jwtSettings.ExpirationMinutes == 0)
{
    jwtSettings.ExpirationMinutes = 60;
}

if (string.IsNullOrEmpty(jwtSettings.Key))
    throw new Exception("JwtSettings.Key no está configurado. Verifica tu appsettings o variables de entorno.");

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtSettings.Key;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.ExpirationMinutes = jwtSettings.ExpirationMinutes;
});

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ApiBizly API",
        Version = "v1",
        Description = "API REST para gestión empresarial con autenticación JWT"
    });

    // Configurar JWT en Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
