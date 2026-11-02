using Workout.Api.Domain;

namespace Workout.Api.Endpoints;

public static class Mapping
{
    public static SetDto ToDto(this LoggedSet s) => new(
        s.Id, s.ExerciseId, s.Exercise.Name, s.SetNumber, s.SetType,
        s.Weight, s.Reps, s.DurationSeconds, s.RestSeconds, s.Notes);

    public static SessionDto ToDto(this Session s) =>
        new(s.Id, s.Date, s.Notes, s.Sets.OrderBy(x => x.Id).Select(x => x.ToDto()).ToList());
}
