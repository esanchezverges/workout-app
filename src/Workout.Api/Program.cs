using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Workout.Api.Data;
using Workout.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WorkoutDbContext>((services, options) =>
    options.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("Workout")));
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// One replica on SQLite, so migrating at startup is safe. Revisit with a real database.
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<WorkoutDbContext>().Database.Migrate();

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
