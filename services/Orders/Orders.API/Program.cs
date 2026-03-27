using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Orders.Application.Repositories;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://192.168.1.90","http://localhost:4200","http://localhost:4201","http://localhost:4202","http://localhost:4203")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

var imagesRootPath = builder.Configuration["Storage:ImagesRootPath"]
    ?? @"C:\Users\Jair\Documents\My Web Sites\LaundrAppBackend\LavanderiaProBackend\services\Orders\images";
var absolutePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, imagesRootPath));
if (!Directory.Exists(absolutePath))
{
    Directory.CreateDirectory(absolutePath);
}
app.UseCors("AllowAngular");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(absolutePath),
    RequestPath = "/images"
});
app.UseAuthentication(); 
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
