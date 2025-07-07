using System.Text;
using APiEcommerce.Constants;
using APiEcommerce.Repository;
using APiEcommerce.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnecionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(Options => Options.UseSqlServer(dbConnecionString));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
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
            ValidateAudience = true
        };
    })
;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// perime el uso de las cors
app.UseCors(PolicyName.AllowSpecifiOrigin);

app.UseAuthorization();

app.MapControllers();

app.Run();
