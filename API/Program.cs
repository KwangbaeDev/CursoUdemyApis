using System.Reflection;
using API.Extensions;
using AspNetCoreRateLimit;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());            // LLamando al servicio AutoMapper.

builder.Services.ConfigureRateLimiting();                               // LLamando al servicio RateLimit.

// Add services to the container.
builder.Services.ConfigureCors();                                       //
builder.Services.AddAplicacionServices();                               // LLamando a los servicios de los repositorios.
builder.Services.ConfigureApiVersioning();                              // Llamando al servicio Versionado.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TiendaContext>(options =>
{
    options.UseNpgsql(connectionString);                                // Llamando al context.
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Configuracion de la negociacion de contenido.
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;                          // Habilita el cambio de formato.
    options.ReturnHttpNotAcceptable = true;                             // Envia un mensaje de error al no soportar el formato solicitado.
}).AddXmlSerializerFormatters();                                        // Se especifica el formato aceptado
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseIpRateLimiting();                                                 // Usando el Ratelimit.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplica cualquier migracion pendiente al iniciar la aplicacion, tambien creara la base datos si es que no existe en ese momento.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<TiendaContext>();
        await context.Database.MigrateAsync();
        await TiendaContextSeed.SeedAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Ocurrio un error durante la migración");
    }
}
// Usando las politicas Cros.
app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

