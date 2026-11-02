using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Workout.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyPart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Equipment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetMuscle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoggedSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    SetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
                    Reps = table.Column<int>(type: "int", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    RestSeconds = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggedSets", x => x.Id);
                    table.CheckConstraint("CK_LoggedSet_FieldsMatchSetType", "(\"SetType\" = 'Weighted' AND \"Weight\" IS NOT NULL AND \"Reps\" IS NOT NULL AND \"DurationSeconds\" IS NULL)\nOR (\"SetType\" = 'Timed' AND \"DurationSeconds\" IS NOT NULL AND \"Weight\" IS NULL AND \"Reps\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_LoggedSets_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoggedSets_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "BodyPart", "Description", "Equipment", "ImageUrl", "Name", "SetType", "TargetMuscle", "VideoUrl" },
                values: new object[,]
                {
                    { 1, "Legs", null, "Barbell", null, "Back Squat", "Weighted", "Quadriceps", null },
                    { 2, "Back", null, "Barbell", null, "Deadlift", "Weighted", "Hamstrings", null },
                    { 3, "Chest", null, "Barbell", null, "Bench Press", "Weighted", "Pectorals", null },
                    { 4, "Shoulders", null, "Barbell", null, "Overhead Press", "Weighted", "Deltoids", null },
                    { 5, "Back", null, "Barbell", null, "Barbell Row", "Weighted", "Lats", null },
                    { 6, "Back", null, "Bodyweight", null, "Pull-Up", "Weighted", "Lats", null },
                    { 7, "Arms", null, "Dumbbell", null, "Dumbbell Curl", "Weighted", "Biceps", null },
                    { 8, "Legs", null, "Barbell", null, "Romanian Deadlift", "Weighted", "Hamstrings", null },
                    { 9, "Core", null, "Bodyweight", null, "Plank", "Timed", "Abdominals", null },
                    { 10, "Core", null, "Bodyweight", null, "Side Plank", "Timed", "Obliques", null },
                    { 11, "Legs", null, "Bodyweight", null, "Wall Sit", "Timed", "Quadriceps", null },
                    { 12, "Full Body", null, "Dumbbell", null, "Farmer's Carry", "Timed", "Forearms", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoggedSets_ExerciseId",
                table: "LoggedSets",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_LoggedSets_SessionId",
                table: "LoggedSets",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoggedSets");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
