using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Orders.Application.Repositories;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
});
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<IOrderNotificationPublisher, OrderNotificationPublisher>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Notifications:BaseUrl"] ?? "http://localhost:5004/api/Notifications/");
});
builder.Services.AddHostedService<OrderNotificationOutboxWorker>();
builder.Services.AddHostedService<ClientOrderSmsNotificationOutboxWorker>();
builder.Services.AddControllers();
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

var app = builder.Build();

await app.Services.ApplyPendingMigrationsAsync<OrdersDbContext>(builder.Configuration, app.Logger);

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
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(absolutePath),
    RequestPath = "/images"
});
app.UseAuthentication(); 
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
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
