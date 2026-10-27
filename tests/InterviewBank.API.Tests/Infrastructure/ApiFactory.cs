using InterviewBank.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace InterviewBank.API.Tests.Infrastructure;

/// <summary>
/// Boots the real API against a throwaway PostgreSQL container.
/// Migrations and seeders run exactly as they do in production.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public FakeAnswerEvaluator Evaluator { get; } = new();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        // Force the host to start so migrations run once, up front
        _ = Server;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-0123456789abcdef");
        builder.UseSetting("Jwt:Issuer", "InterviewBank.API");
        builder.UseSetting("Jwt:Audience", "InterviewBank.UI");
        builder.UseSetting("AllowedOrigin", "http://localhost:4200");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAnswerEvaluator>();
            services.AddSingleton<IAnswerEvaluator>(Evaluator);
        });
    }

    /// <summary>HTTPS base address so the Secure refresh-token cookie round-trips.</summary>
    public HttpClient CreateApiClient(bool handleCookies = true) =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress       = new Uri("https://localhost"),
            HandleCookies     = handleCookies,
            AllowAutoRedirect = false
        });
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
