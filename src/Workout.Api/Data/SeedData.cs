using Workout.Api.Domain;

namespace Workout.Api.Data;

public static class SeedData
{
    public static readonly Exercise[] Exercises =
    [
        Weighted(1, "Back Squat", "Legs", "Barbell", "Quadriceps"),
        Weighted(2, "Deadlift", "Back", "Barbell", "Hamstrings"),
        Weighted(3, "Bench Press", "Chest", "Barbell", "Pectorals"),
        Weighted(4, "Overhead Press", "Shoulders", "Barbell", "Deltoids"),
        Weighted(5, "Barbell Row", "Back", "Barbell", "Lats"),
        Weighted(6, "Pull-Up", "Back", "Bodyweight", "Lats"),
        Weighted(7, "Dumbbell Curl", "Arms", "Dumbbell", "Biceps"),
        Weighted(8, "Romanian Deadlift", "Legs", "Barbell", "Hamstrings"),
        Timed(9, "Plank", "Core", "Bodyweight", "Abdominals"),
        Timed(10, "Side Plank", "Core", "Bodyweight", "Obliques"),
        Timed(11, "Wall Sit", "Legs", "Bodyweight", "Quadriceps"),
        Timed(12, "Farmer's Carry", "Full Body", "Dumbbell", "Forearms"),
    ];

    private static Exercise Weighted(int id, string name, string bodyPart, string equipment, string muscle) =>
        new() { Id = id, Name = name, BodyPart = bodyPart, Equipment = equipment, TargetMuscle = muscle, SetType = SetType.Weighted };

    private static Exercise Timed(int id, string name, string bodyPart, string equipment, string muscle) =>
        new() { Id = id, Name = name, BodyPart = bodyPart, Equipment = equipment, TargetMuscle = muscle, SetType = SetType.Timed };
}
