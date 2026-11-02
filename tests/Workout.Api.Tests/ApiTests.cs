using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workout.Api.Data;
using Workout.Api.Domain;
using Workout.Api.Endpoints;

namespace Workout.Api.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private const int BenchPress = 3;
    private const int Deadlift = 2;
    private const int Plank = 9;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static NewSetRequest Weighted(int exerciseId, decimal weight = 100, int reps = 5) =>
        new(exerciseId, weight, reps, null, 120, null);

    private static NewSetRequest Timed(int exerciseId, int seconds = 60) =>
        new(exerciseId, null, null, seconds, 30, null);

    private Task<HttpResponseMessage> Post(params NewSetRequest[] sets) =>
        _client.PostAsJsonAsync("/api/sessions", new NewSessionRequest(new DateOnly(2026, 11, 2), null, [.. sets]), Json);

    [Fact]
    public async Task Healthz_returns_ok()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Exercises_include_the_seeded_catalog()
    {
        var exercises = await _client.GetFromJsonAsync<List<ExerciseDto>>("/api/exercises", Json);

        Assert.Equal(SeedData.Exercises.Length, exercises!.Count);
        Assert.Contains(exercises, e => e.Name == "Plank" && e.SetType == SetType.Timed);
    }

    [Fact]
    public async Task Posting_a_weighted_set_stores_it_and_numbers_sets_per_exercise()
    {
        var response = await Post(Weighted(BenchPress), Timed(Plank), Weighted(BenchPress, 105, 3));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SessionDto>(Json);
        var stored = await _client.GetFromJsonAsync<SessionDto>(response.Headers.Location, Json);

        Assert.Equal(3, stored!.Sets.Count);
        Assert.Equal(created!.Id, stored.Id);
        Assert.Equal([1, 1, 2], stored.Sets.Select(s => s.SetNumber));
        Assert.Equal(105, stored.Sets[2].Weight);
        Assert.Equal(SetType.Weighted, stored.Sets[0].SetType);
    }

    [Fact]
    public async Task Posting_a_timed_set_stores_its_duration()
    {
        var response = await Post(Timed(Plank, 90));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SessionDto>(Json);
        Assert.Equal(90, created!.Sets.Single().DurationSeconds);
        Assert.Null(created.Sets.Single().Weight);
    }

    public static TheoryData<NewSetRequest> InvalidSets => new()
    {
        new NewSetRequest(BenchPress, null, null, 60, null, null), // duration on a weighted exercise
        new NewSetRequest(BenchPress, 100, null, null, null, null), // missing reps
        new NewSetRequest(BenchPress, -1, 5, null, null, null), // negative weight
        new NewSetRequest(BenchPress, 100, 0, null, null, null), // zero reps
        new NewSetRequest(Plank, 20, 5, null, null, null), // weight and reps on a timed exercise
        new NewSetRequest(Plank, null, null, 0, null, null), // zero duration
        new NewSetRequest(Plank, null, null, 60, -1, null), // negative rest
        new NewSetRequest(9999, 100, 5, null, null, null), // unknown exercise
    };

    [Theory]
    [MemberData(nameof(InvalidSets))]
    public async Task Invalid_sets_are_rejected(NewSetRequest set)
    {
        var response = await Post(set);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_session_without_sets_is_rejected()
    {
        var response = await Post();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_database_rejects_fields_that_do_not_match_the_set_type()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
        db.Sessions.Add(new Session
        {
            Date = new DateOnly(2026, 11, 2),
            Sets = [new LoggedSet { ExerciseId = Plank, SetNumber = 1, SetType = SetType.Timed, Weight = 10 }],
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task History_returns_logged_sets_newest_first()
    {
        await _client.PostAsJsonAsync("/api/sessions", new NewSessionRequest(
            new DateOnly(2026, 1, 1), null, [Weighted(Deadlift, 140)]), Json);
        await _client.PostAsJsonAsync("/api/sessions", new NewSessionRequest(
            new DateOnly(2026, 2, 1), null, [Weighted(Deadlift, 150), Weighted(BenchPress)]), Json);

        var history = (await _client.GetFromJsonAsync<List<ExerciseHistoryEntryDto>>(
            $"/api/exercises/{Deadlift}/history", Json))!;

        Assert.Equal([new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)], history.Select(h => h.Date));
        Assert.Equal(150, history[0].Sets.Single().Weight);
        Assert.All(history.SelectMany(h => h.Sets), s => Assert.Equal(Deadlift, s.ExerciseId));
    }

    [Fact]
    public async Task Unknown_ids_and_api_routes_return_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/exercises/9999/history")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/sessions/9999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/nope")).StatusCode);
    }
}
