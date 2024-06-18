using System.Reflection;
using API.Extensions;
using API.Helpers.Errors;
using AspNetCoreRateLimit;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .CreateLogger();

//builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());            // LLamando al servicio AutoMapper.

builder.Services.ConfigureRateLimiting();                               // LLamando al servicio RateLimit.

// Add services to the container.
builder.Services.ConfigureCors();                                       //
builder.Services.AddAplicacionServices();                               // LLamando a los servicios de los repositorios de ApplicationServiceExtensions.
builder.Services.ConfigureApiVersioning();                              // Llamando al servicio Versionado de ApplicationServiceExtensions.
builder.Services.AddJwt(builder.Configuration);                         // LLamando al servicio para Tokens de ApplicationServiceExtensions.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");     //crea la cadena de conexion a la base de datos.

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

builder.Services.AddValidationErrors();                                 // LLamando al servicio de Validacion de ApplicationServiceExtensions.
                                      
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();   

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

app.UseMiddleware<ExceptionMiddleware>();                                //Usando el Middleware para manejar las excepciones de mandera global.

app.UseStatusCodePagesWithReExecute("/errors/{0}");                      // Para validacion de recursos no existentes(endpoints, etc) que se llama desde el controlador errors.

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
        await TiendaContextSeed.SeedAsync(context, loggerFactory);               // Llamada a los metodos.
        await TiendaContextSeed.SeedRolesAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var _logger = loggerFactory.CreateLogger<Program>();
        _logger.LogError(ex, "Ocurrio un error durante la migración");
    }
}
// Usando las politicas Cros.
app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

