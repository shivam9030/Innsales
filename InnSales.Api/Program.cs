using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InnSales.Domain.UserManagement.Entities;
using InnSales.DataBase;
using InnSales.Services;
using InnSales.MappingProfiles;
using Stripe;
using MockEventGrid;
using InnSales.Services.Authentication.EventGrid;
using InnSales.Services.Authentication.Webhooks;
using InnSales.Api;
var builder = WebApplication.CreateBuilder(args);


// Add DbContext
builder.Services.AddDbContext<InnSalesDbContext>(options =>
    options.UseSqlServer("Server=sqlserver,1433;Database=InnSalesDB;User Id=sa;Password=1shivam2;TrustServerCertificate=True"));


// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<InnSalesDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
// Add JWT Authentication using consistent keys
var jwtKey = builder.Configuration["JWT_SECRET"] ?? "SuperLongSecretKeyForJwtMustBe32CharsMin";
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "InnSalesIssuer";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "InnSalesAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});


// AutoMapper Profiles
builder.Services.AddAutoMapper(typeof(OrderProfile));
builder.Services.AddAutoMapper(typeof(PaymentProfile));
builder.Services.AddAutoMapper(typeof(NewsMappingProfile));
builder.Services.AddAutoMapper(typeof(BasketProfile));
builder.Services.AddAutoMapper(typeof(ProductProfile)); // or typeof(MapProfile)

// Stripe Configuration
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Register Services
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductsService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPromoCodeService, PromoCodeService>();
builder.Services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();
builder.Services.AddScoped<IPaymentTokenService, PaymentTokenService>();

// For InnSales.Services (Order service host) or where you host background workers// For InnSales.Services (Order
builder.Services.AddScoped<IPaymentTransactionPublisher,PaymentTransactionPublisher>();
builder.Services.AddHostedService<PaymentTimeoutService>();
builder.Services.AddSingleton(new MockEventGridBroker("UseDevelopmentStorage=true"));

// Runs the Service Bus listener in the API host
builder.Services.AddHostedService<PaymentTransactionProcessor>();

builder.Services.AddSingleton<IAuthEventGridProvisioningService, AuthEventGridProvisioningService>();
builder.Services.AddScoped<IAuthWebhookRegistrationService, AuthWebhookRegistrationService>();


// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Swagger Configuration
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "InnSales API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "InnSales API", Version = "v2" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();



// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "InnSales API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "InnSales API V2");
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();