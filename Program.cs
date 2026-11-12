using System.Text;
using Inventory_Flow.Components;
using Inventory_Flow.Data;
using Inventory_Flow.Mapping;
using Inventory_Flow.Repository;
using Inventory_Flow.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la base de datos
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<AplicationDbContext>(options => 
    options.UseSqlServer(dbConnectionString));

// 2. Registro del Repositorio (Inyección de dependencias)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IMovementTypeRepository, MovementTypeRepository>();

// 3. AutoMapper
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<RoleProfile>();
    cfg.AddProfile<WarehouseProfile>();
    cfg.AddProfile<ProductProfile>();
    cfg.AddProfile<CategoryProfile>();
    cfg.AddProfile<LotProfile>();
    cfg.AddProfile<MovementTypeProfile>();
});

var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
  throw new InvalidOperationException("SecretKey no esta configurada");
}
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.RequireHttpsMetadata = false;
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    ValidateIssuer = false,
    ValidateAudience = false
  };
})
;
// 4. Controladores
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 5. Configuración de Swagger (UI interactiva)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
    builder =>
    {
      builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();  
    }
    );
}
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
// 6. Pipeline HTTP y habilitación de Swagger en Desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
