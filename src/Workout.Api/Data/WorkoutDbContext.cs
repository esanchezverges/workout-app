using Microsoft.EntityFrameworkCore;
using Workout.Api.Domain;

namespace Workout.Api.Data;

public class WorkoutDbContext(DbContextOptions<WorkoutDbContext> options) : DbContext(options)
{
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<LoggedSet> LoggedSets => Set<LoggedSet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.SetType).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasData(SeedData.Exercises);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasMany(x => x.Sets).WithOne(x => x.Session).HasForeignKey(x => x.SessionId);
        });

        modelBuilder.Entity<LoggedSet>(e =>
        {
            e.Property(x => x.SetType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Weight).HasPrecision(7, 2);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasOne(x => x.Exercise).WithMany().HasForeignKey(x => x.ExerciseId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint(
                "CK_LoggedSet_FieldsMatchSetType",
                """
                ("SetType" = 'Weighted' AND "Weight" IS NOT NULL AND "Reps" IS NOT NULL AND "DurationSeconds" IS NULL)
                OR ("SetType" = 'Timed' AND "DurationSeconds" IS NOT NULL AND "Weight" IS NULL AND "Reps" IS NULL)
                """));
        });
    }
}
