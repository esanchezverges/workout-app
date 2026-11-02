using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Workout.Api.Data;

namespace Workout.Api.Tests;

/// <summary>Runs the real app and migrations against a throwaway SQL Server container.</summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<WorkoutDbContext>().Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connection = new SqlConnectionStringBuilder(_database.GetConnectionString())
        {
            InitialCatalog = "workout_test",
        };

        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:Workout"] = connection.ConnectionString }));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
