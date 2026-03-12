using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FisaBank.Api.Data;
using FisaBank.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "FisaBank API",
        Version     = "v1",
        Description = "API de pagos y comisiones para contratistas independientes. Fisapay x Toyota Financial Services Colombia 2026.",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Ingresa tu token: Bearer {token}",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<FisaBankDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=fisabank.db"));

builder.Services.AddScoped<JwtService>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "FisaBankSuperSecretKey2026!@#$%^";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer      = true,
            ValidIssuer         = "fisabank-api",
            ValidateAudience    = true,
            ValidAudience       = "fisabank-clients",
            ValidateLifetime    = true,
            IssuerSigningKey    = signingKey,
            RequireSignedTokens = false,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opt =>
    opt.AddPolicy("OpenPolicy", p =>
        p.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FisaBankDbContext>();
    DatabaseSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FisaBank API v1");
    c.RoutePrefix   = "swagger";
    c.DocumentTitle = "FisaBank v1";
});

app.UseCors("OpenPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
