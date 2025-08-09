using FarmErp.Api.Data;
using Microsoft.EntityFrameworkCore;

const string CorsPolicy = "DefaultCors";

var builder = WebApplication.CreateBuilder(args);

// ------------------------ Services ------------------------
builder.Services
    .AddControllers()
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ProblemDetails
builder.Services.AddProblemDetails();

// EF Core + SQL Server + NetTopologySuite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? "Server=(localdb)\\mssqllocaldb;Database=FarmErpDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.UseNetTopologySuite())
);

var app = builder.Build();

// ------------------------ Middleware ------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseExceptionHandler(); 
}

app.UseCors(CorsPolicy);
app.UseAuthorization();

// ------------------------ Endpoints ------------------------
app.MapControllers();

// Healthcheck
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
