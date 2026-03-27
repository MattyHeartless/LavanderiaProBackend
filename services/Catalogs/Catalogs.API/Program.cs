using Catalogs.Infrastructure.Persistence;
using Catalogs.Infrastructure.Repositories;
using Catalogs.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<CatalogsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CatalogsDb")
    )
);
builder.Services.AddScoped<ICatalogsRepository,CatalogsRepository >();
builder.Services.AddScoped<CatalogsService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
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

builder.Services.AddControllers();
var app = builder.Build();

var imagesRootPath = builder.Configuration["Storage:ImagesRootPath"]
    ?? @"C:\Users\Jair\Documents\My Web Sites\LaundrAppBackend\LavanderiaProBackend\services\Catalogs\images";
var absolutePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, imagesRootPath));
if (!Directory.Exists(absolutePath))
{
    Directory.CreateDirectory(absolutePath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(absolutePath),
    RequestPath = "/images"
});
app.UseAuthorization();
app.UseCors("AllowAngular");
app.MapControllers();
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
