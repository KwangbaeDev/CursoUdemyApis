using System.Text;
using API.Helpers;
using API.Helpers.Errors;
using API.Services;
using AspNetCoreRateLimit;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    //Configuracion o metodo de extension para Cors().
    public static void ConfigureCors(this IServiceCollection services) =>
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", builder =>
                        builder.AllowAnyOrigin()      //WithOrigins("https://dominio.com")
                               .AllowAnyMethod()      //WithMethods("GET","POST")  
                               .AllowAnyHeader());    //WithHeaders("accept","content-type")
                });



    public static void AddAplicacionServices(this IServiceCollection services)
    {
        // services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        // services.AddScoped<IProductoRepository, ProductoRepository>();
        // services.AddScoped<IMarcaRepository, MarcaRepository>();
        // services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }


    // Configuracion o metodo de extension para el RateLimit.
    public static void ConfigureRateLimiting(this IServiceCollection services)
    {

        services.AddMemoryCache();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();

        services.Configure<IpRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.StackBlockedRequests = false;
            options.HttpStatusCode = 429;
            options.RealIpHeader = "X-Real-IP";
            options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "10s",
                    Limit = 2
                }
            };
        });
    }


    //Configuracion o metodo de extension para el versionado.
    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            // options.ApiVersionReader = new UrlSegmentApiVersionReader();                // Indica el versionado por Url.
            // options.ApiVersionReader = new QueryStringApiVersionReader("v");            // Indica el versionado por QueryString.
            // options.ApiVersionReader = new HeaderApiVersionReader("X-Version");         // Indica el versionado por Header o Encabezado.
            options.ApiVersionReader = ApiVersionReader.Combine(                           // Combina las versiones para poder utilizar varias
                new QueryStringApiVersionReader("v"),                                      // al mismo tiempo.
                new HeaderApiVersionReader("X-Version")
            );
            options.ReportApiVersions = true;
        });/*.AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";                                               // Solo en caso de usar versionado por Url.
            options.SubstituteApiVersionInUrl = true;
        });*/
    }


    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuraiton from AppSettings
        services.Configure<JWT>(configuration.GetSection("JWT"));

        // Adding Athentication - JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };
            });
    }


    // Accediendo al ModelState para manejar validaciones.
    public static void AddValidationErrors(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                var errors = actionContext.ModelState.Where(me => me.Value.Errors.Count>0)   //busca si hay errores.
                                                .SelectMany(me => me.Value.Errors)           //se utiliza SelecMany ya que Errors es una coleccion de errores.
                                                .Select(me => me.ErrorMessage).ToArray();

                var errorResponse = new ApiValidation()
                {
                    Errors = errors
                };

                return new BadRequestObjectResult(errorResponse);
            };
        });
    }
}