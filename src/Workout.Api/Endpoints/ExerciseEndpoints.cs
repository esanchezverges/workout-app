using Microsoft.EntityFrameworkCore;
using Workout.Api.Data;

namespace Workout.Api.Endpoints;

public static class ExerciseEndpoints
{
    public static void MapExerciseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exercises");

        group.MapGet("/", async (WorkoutDbContext db, CancellationToken ct) =>
            await db.Exercises.AsNoTracking()
                .OrderBy(e => e.Name)
                .Select(e => new ExerciseDto(
                    e.Id, e.Name, e.Description, e.BodyPart, e.Equipment,
                    e.TargetMuscle, e.ImageUrl, e.VideoUrl, e.SetType))
                .ToListAsync(ct));

        group.MapGet("/{id:int}/history", async (int id, WorkoutDbContext db, CancellationToken ct) =>
        {
            if (!await db.Exercises.AnyAsync(e => e.Id == id, ct))
                return Results.NotFound();

            var sets = await db.LoggedSets.AsNoTracking()
                .Include(s => s.Session)
                .Include(s => s.Exercise)
                .Where(s => s.ExerciseId == id)
                .OrderBy(s => s.SetNumber)
                .ToListAsync(ct);

            var history = sets
                .GroupBy(s => s.Session)
                .OrderByDescending(g => g.Key.Date).ThenByDescending(g => g.Key.Id)
                .Select(g => new ExerciseHistoryEntryDto(g.Key.Id, g.Key.Date, g.Select(s => s.ToDto()).ToList()));

            return Results.Ok(history);
        });
    }
}
