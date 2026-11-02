using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Workout.Api.Data;
using Workout.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WorkoutDbContext>((services, options) =>
    options.UseSqlServer(
        services.GetRequiredService<IConfiguration>().GetConnectionString("Workout"),
        // Azure SQL serverless pauses when idle; retries cover the resume.
        sql => sql.EnableRetryOnFailure(maxRetryCount: 6, maxRetryDelay: TimeSpan.FromSeconds(15), errorNumbersToAdd: null)));
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Migrations run as a separate step so that app startup never wakes the paused database.
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<WorkoutDbContext>().Database.Migrate();
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapExerciseEndpoints();
app.MapSessionEndpoints();

// Unknown API routes stay 404; everything else goes to the React app.
app.MapFallback("/api/{**rest}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
