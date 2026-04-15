
using Microsoft.EntityFrameworkCore;
using OrderMicroservice.Database;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------
// 1. Add services to DI container
// -------------------------------------
builder.Services.AddControllers();

// Register OrderDbContext with SQL Server
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrderDb")
    );

    // Helpful during development (remove later in production)
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Optional: Add Health Checks (you can remove if not needed)
builder.Services.AddHealthChecks();

var app = builder.Build();

// -------------------------------------
// 2. Configure the HTTP request pipeline
// -------------------------------------

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
