using Workout.Api.Domain;

namespace Workout.Api.Endpoints;

public record ExerciseDto(
    int Id, string Name, string? Description, string BodyPart, string Equipment,
    string TargetMuscle, string? ImageUrl, string? VideoUrl, SetType SetType);

public record NewSetRequest(
    int ExerciseId, decimal? Weight, int? Reps, int? DurationSeconds, int? RestSeconds, string? Notes);

public record NewSessionRequest(DateOnly Date, string? Notes, List<NewSetRequest>? Sets);

public record SetDto(
    int Id, int ExerciseId, string ExerciseName, int SetNumber, SetType SetType,
    decimal? Weight, int? Reps, int? DurationSeconds, int? RestSeconds, string? Notes);

public record SessionDto(int Id, DateOnly Date, string? Notes, List<SetDto> Sets);

public record SessionSummaryDto(int Id, DateOnly Date, string? Notes, int SetCount, List<string> Exercises);

public record ExerciseHistoryEntryDto(int SessionId, DateOnly Date, List<SetDto> Sets);
