using Inventory_Flow.Data;
using Inventory_Flow.Mapping;
using Inventory_Flow.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la base de datos
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<AplicationDbContext>(options => 
    options.UseSqlServer(dbConnectionString));

// 2. Registro del Repositorio (Inyección de dependencias)
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 3. AutoMapper
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<UserProfile>();
});
// 4. Controladores
builder.Services.AddControllers();

// 5. Configuración de Swagger (UI interactiva)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
