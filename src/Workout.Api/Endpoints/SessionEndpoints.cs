using Microsoft.EntityFrameworkCore;
using Workout.Api.Data;
using Workout.Api.Domain;

namespace Workout.Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions");

        group.MapGet("/", async (WorkoutDbContext db, CancellationToken ct) =>
        {
            var sessions = await db.Sessions.AsNoTracking()
                .Include(s => s.Sets).ThenInclude(s => s.Exercise)
                .OrderByDescending(s => s.Date).ThenByDescending(s => s.Id)
                .ToListAsync(ct);

            return sessions.Select(s => new SessionSummaryDto(
                s.Id, s.Date, s.Notes, s.Sets.Count,
                s.Sets.Select(x => x.Exercise.Name).Distinct().ToList()));
        });

        group.MapGet("/{id:int}", async (int id, WorkoutDbContext db, CancellationToken ct) =>
        {
            var session = await db.Sessions.AsNoTracking()
                .Include(s => s.Sets).ThenInclude(s => s.Exercise)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            return session is null ? Results.NotFound() : Results.Ok(session.ToDto());
        });

        group.MapPost("/", async (NewSessionRequest request, WorkoutDbContext db, CancellationToken ct) =>
        {
            var sets = request.Sets ?? [];
            var ids = sets.Select(s => s.ExerciseId).Distinct().ToList();
            var exercises = await db.Exercises.Where(e => ids.Contains(e.Id)).ToDictionaryAsync(e => e.Id, ct);

            var errors = Validate(request, sets, exercises);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var session = new Session { Date = request.Date, Notes = request.Notes };
            var nextSetNumber = new Dictionary<int, int>();
            foreach (var set in sets)
            {
                var number = nextSetNumber.GetValueOrDefault(set.ExerciseId) + 1;
                nextSetNumber[set.ExerciseId] = number;
                session.Sets.Add(new LoggedSet
                {
                    Exercise = exercises[set.ExerciseId],
                    SetNumber = number,
                    SetType = exercises[set.ExerciseId].SetType,
                    Weight = set.Weight,
                    Reps = set.Reps,
                    DurationSeconds = set.DurationSeconds,
                    RestSeconds = set.RestSeconds,
                    Notes = set.Notes,
                });
            }

            db.Sessions.Add(session);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/sessions/{session.Id}", session.ToDto());
        });
    }

    private static Dictionary<string, string[]> Validate(
        NewSessionRequest request, List<NewSetRequest> sets, Dictionary<int, Exercise> exercises)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Date == default)
            errors["date"] = ["A date is required."];
        if (request.Notes?.Length > 1000)
            errors["notes"] = ["Notes can be at most 1000 characters."];
        if (sets.Count == 0)
            errors["sets"] = ["At least one set is required."];

        for (var i = 0; i < sets.Count; i++)
        {
            var set = sets[i];
            var problems = new List<string>();

            if (!exercises.TryGetValue(set.ExerciseId, out var exercise))
            {
                problems.Add($"Exercise {set.ExerciseId} does not exist.");
            }
            else if (exercise.SetType == SetType.Weighted)
            {
                if (set.Weight is null or < 0 or > 99999) problems.Add("Weight must be between 0 and 99999.");
                if (set.Reps is null or < 1) problems.Add("Reps must be at least 1.");
                if (set.DurationSeconds is not null) problems.Add("A weighted set cannot have a duration.");
            }
            else
            {
                if (set.DurationSeconds is null or < 1) problems.Add("Duration must be at least 1 second.");
                if (set.Weight is not null || set.Reps is not null) problems.Add("A timed set cannot have weight or reps.");
            }

            if (set.RestSeconds < 0) problems.Add("Rest cannot be negative.");
            if (set.Notes?.Length > 500) problems.Add("Notes can be at most 500 characters.");

            if (problems.Count > 0)
                errors[$"sets[{i}]"] = [.. problems];
        }

        return errors;
    }
}
