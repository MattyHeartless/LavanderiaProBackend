using Microsoft.EntityFrameworkCore;
using Profile.Application.Interfaces;
using Profile.Infrastructure.Persistence;
using Profile.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProfileConnection")
    ));
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://192.168.100.53", "http://localhost:4200", "http://localhost:4201", "http://localhost:4202", "http://localhost:4203",
                    "http://bcl8c20hq8435ht1ejz3u0mf.5.78.222.52.sslip.io",
                    "http://p2iw9w364eunl2tyotiihcrx.5.78.222.52.sslip.io",
                    "http://qptlq1wl33v86u3y09b5xfhp.5.78.222.52.sslip.io",
                    "https://lavanderiaatucasa.com",
                    "https://admin.lavanderiaatucasa.com",
                    "https://reparto.lavanderiaatucasa.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddControllers();
var app = builder.Build();

await app.Services.ApplyPendingMigrationsAsync<ProfileDbContext>(builder.Configuration, app.Logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
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
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
