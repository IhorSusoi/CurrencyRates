using CurrencyRates.Infrastructure.Data;
using CurrencyRates.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,  // новий файл кожен день
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Запуск застосунку...");

    var builder = WebApplication.CreateBuilder(args);

    // Підключаємо Serilog замість стандартного логування
    builder.Host.UseSerilog();

    // Реєструємо всі сервіси Infrastructure (БД, НБУ клієнт, репозиторій...)
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers();

    // Swagger — документація API
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Currency Rates API",
            Version = "v1",
            Description = "API для отримання курсів валют НБУ"
        });

        // Підключаємо XML коментарі до Swagger
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    // CORS — дозволяємо Blazor клієнту звертатись до API
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BlazorClient", policy =>
            policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    // Автоматично застосовуємо міграції при старті
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Міграції застосовано успішно");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("BlazorClient");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Застосунок впав при старті");
}
finally
{
    Log.CloseAndFlush();
}