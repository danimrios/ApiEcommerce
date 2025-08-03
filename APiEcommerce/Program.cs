using System.Text;
using System.Text.Json.Serialization;
using APiEcommerce.Constants;
using APiEcommerce.Data;
using APiEcommerce.Models;
using APiEcommerce.Repository;
using APiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnecionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(dbConnecionString)
  .UseSeeding((context, _) =>
  {
    var appContext = (ApplicationDbContext)context;
    DataSeeder.SeedData(appContext);
  })
  );
//maneja el tamaño de lo que se guarde en cache y tambien maneja si va a ser sencible a mayusculas y minusculas
  builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024;
    options.UseCaseSensitivePaths = true;
});
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
///este es u sistema robusto de asp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var secreteKey = builder.Configuration.GetValue<string>(PolicyName.SecretKey);
if (string.IsNullOrEmpty(secreteKey))
{
    throw new InvalidOperationException("secretekey no esta configurada correctamente");
}
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(options =>
    {      
        options.RequireHttpsMetadata = false;//en el caso de produccion esto va en true, es la valiacion de http
        options.SaveToken = true;
        //aca se define los parametros
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            //aca se verifica que se use el grabado de token que se configuro 
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreteKey)),
            //no se valida el emisor del token 
            ValidateIssuer = false,
            //no se va a validar el publico del token 
            ValidateAudience = false
        };
    })
;

builder.Services.AddControllers(option=>
{
  option.CacheProfiles.Add("Default10", new CacheProfile()
  {
    Duration = 10,
  });
    option.CacheProfiles.Add(CacheProfiles.Default60, CacheProfiles.Profile60);
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
     //esto va a permitir que swager puea utilizar token
     options =>
  {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
      Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
                    "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
                    "Ejemplo: \"12345abcdef\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
          },
          Scheme = "oauth2",
          Name = "Bearer",
          In = ParameterLocation.Header
        },
        new List<string>()
      }
    });/*
    options.SwaggerDoc("v1", new OpenApiInfo
    {
      Version = "v1",
      Title = "API Ecommerce",
      Description = "ai para gestion de productos",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "Ecommerce",
        Url = new Uri("http://example.com/contact"),
      },
      License = new OpenApiLicense
      {
        Name = "lecencia de uso",
        Url = new Uri("http://example.com/licencs")
      },

    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
      Version = "v2",
      Title = "API Ecommerce",
      Description = "ai para gestion de productos",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "Ecommerce",
        Url = new Uri("http://example.com/contact"),
      },
      License = new OpenApiLicense
      {
        Name = "lecencia de uso",
        Url = new Uri("http://example.com/licencs")
      },

    });
    options.DocInclusionPredicate((version, desc) =>
    
{
  
    if (!desc.TryGetMethodInfo(out var methodInfo))
      return false;

    var versions = methodInfo
        .DeclaringType?
        .GetCustomAttributes(true)
        .OfType<ApiVersionAttribute>()
        .SelectMany(attr => attr.Versions);

    return versions?.Any(v => $"v{v.ToString()}" == version) ?? false;
});
*/
 options.SwaggerDoc("v1", new OpenApiInfo
    {
      Version = "v1",
      Title = "API Ecommerce",
      Description = "API para gestionar productos y usuarios",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "DevTalles",
        Url = new Uri("https://devtalles.com")
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("http://example.com/license")
      }
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
      Version = "v2",
      Title = "API Ecommerce V2",
      Description = "API para gestionar productos y usuarios",
      TermsOfService = new Uri("http://example.com/terms"),
      Contact = new OpenApiContact
      {
        Name = "DevTalles",
        Url = new Uri("https://devtalles.com")
      },
      License = new OpenApiLicense
      {
        Name = "Licencia de uso",
        Url = new Uri("http://example.com/license")
      }
    });
  
  }
);

//versionamiento de la api

var ApiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
  option.AssumeDefaultVersionWhenUnspecified = true;
  option.DefaultApiVersion = new ApiVersion(1, 0);
  option.ReportApiVersions = true;
  //esta obliga a que se introdusca la version a usar
  //options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));
});
ApiVersioningBuilder.AddApiExplorer(option =>
{
  option.GroupNameFormat = "'v'VVV"; // v1,v2,v3...
  option.SubstituteApiVersionInUrl = true; // api/v{version}/products
});

//declaracion de las politicas de cors
builder.Services.AddCors(Options =>
    {
      Options.AddPolicy(PolicyName.AllowSpecifiOrigin,
      builder =>
      {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
      }
      );
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(

      options =>
    {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
      options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}
//esto habilita el uso de los archivo estaticos!
app.UseStaticFiles();

app.UseHttpsRedirection();
// perime el uso de las cors
app.UseCors(PolicyName.AllowSpecifiOrigin);
//habilita el uso de cache
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
