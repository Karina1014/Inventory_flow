using System.Text;
using Inventory_Flow.Components;
using Inventory_Flow.Data;
using Inventory_Flow.Mapping;
using Inventory_Flow.Repository;
using Inventory_Flow.Repository.IRepository;
using Inventory_Flow.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la base de datos (Transient para prevenir concurrencia en Blazor Server)
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<AplicationDbContext>(options => 
    options.UseSqlServer(dbConnectionString), ServiceLifetime.Transient);

// 2. Registro del Repositorio (Inyección de dependencias)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Inventory_Flow.Services.IToastNotificationService, Inventory_Flow.Services.ToastNotificationService>();
builder.Services.AddScoped<Inventory_Flow.Services.IUserContextService, Inventory_Flow.Services.UserContextService>();
builder.Services.AddScoped<Inventory_Flow.Services.CurrentUserService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<ILotRepository, LotRepository>();
builder.Services.AddTransient<IMovementTypeRepository, MovementTypeRepository>();
builder.Services.AddTransient<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();
builder.Services.AddTransient<IThresholdInvetoryRepository, ThresholdInvetoryRepository>();
builder.Services.AddTransient<IMenuRepository, MenuRepository>();
builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<IRequisitionCatalogRepository, RequisitionCatalogRepository>();
builder.Services.AddTransient<IInternalRequisitionRepository, InternalRequisitionRepository>();
builder.Services.AddTransient<IPurchaseOrderCatalogRepository, PurchaseOrderCatalogRepository>();
builder.Services.AddTransient<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IN8nNotificationRepository, N8nNotificationRepository>();
builder.Services.AddTransient<IN8nNotificationService, N8nNotificationService>();
builder.Services.AddTransient<IPurchaseOrderReportService, PurchaseOrderReportService>();
builder.Services.AddTransient<ILowStockReportService, LowStockReportService>();
builder.Services.AddTransient<IMatrixStockReportService, MatrixStockReportService>();
builder.Services.AddTransient<IExpirationAlertService, ExpirationAlertService>();

// 3. AutoMapper
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<RoleProfile>();
    cfg.AddProfile<WarehouseProfile>();
    cfg.AddProfile<ProductProfile>();
    cfg.AddProfile<CategoryProfile>();
    cfg.AddProfile<LotProfile>();
    cfg.AddProfile<MovementTypeProfile>();
    cfg.AddProfile<InventoryMovementProfile>();
    cfg.AddProfile<ThresholdInvetoryProfile>();
    cfg.AddProfile<SupplierProfile>();
    cfg.AddProfile<InternalRequisitionProfile>();
    cfg.AddProfile<PurchaseOrderProfile>();
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
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 7. Semilla de datos (Roles y Menús)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();
    try
    {
        await DbSeeder.SeedAsync(db);
    }
    catch
    {
        // Si la migración aún no ha sido aplicada, continúa sin bloquear el inicio
    }
}

app.Run();
