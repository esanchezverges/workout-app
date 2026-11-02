namespace Workout.Api.Domain;

public enum SetType
{
    Weighted,
    Timed,
}

public class Exercise
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string BodyPart { get; set; }
    public required string Equipment { get; set; }
    public required string TargetMuscle { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public SetType SetType { get; set; }
}

public class Session
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public List<LoggedSet> Sets { get; set; } = [];
}

/// <summary>What was actually done. Weight and Reps apply to Weighted sets, DurationSeconds to Timed.</summary>
public class LoggedSet
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public SetType SetType { get; set; }
    public decimal? Weight { get; set; }
    public int? Reps { get; set; }
    public int? DurationSeconds { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
}
