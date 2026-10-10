using System.Text;
using Almacen.Api.Repositories;
using Almacen.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// ============================================================
// Serilog Ã¢â‚¬â€ logging estructurado
// ============================================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ============================================================
// Controllers
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================================
// Swagger con soporte para JWT Bearer
// ============================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlmacÃƒÂ©n MÃƒÂ³vil Ã¢â‚¬â€ API REST",
        Version = "v1",
        Description = "API para la app mÃƒÂ³vil de inventario educativo offline (Android)."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Introduce el JWT aquÃƒÂ­. Formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// JWT
// ============================================================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurada en user-secrets.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "almacen-mobile-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "almacen-mobile-app";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ============================================================
// InyecciÃƒÂ³n de dependencias
// ============================================================
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IBienRepository, BienRepository>();
    builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ISyncRepository, SyncRepository>();

// ============================================================
// Pipeline
// ============================================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlmacÃƒÂ©n MÃƒÂ³vil API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("DevPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check
app.MapGet("/health", () => new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
});

Log.Information("API AlmacÃƒÂ©n MÃƒÂ³vil iniciada correctamente");
app.Run();