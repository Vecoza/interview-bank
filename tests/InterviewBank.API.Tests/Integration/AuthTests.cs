using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InterviewBank.API.Tests.Infrastructure;

namespace InterviewBank.API.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthTests(ApiFactory factory)
{
    [Fact]
    public async Task Health_endpoint_is_public()
    {
        var res = await factory.CreateApiClient().GetAsync("/health");

        await res.EnsureStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Protected_endpoints_require_a_token()
    {
        var res = await factory.CreateApiClient().GetAsync("/api/questions");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Register_returns_token_and_sets_http_only_refresh_cookie()
    {
        var client = factory.CreateApiClient();
        var email  = TestApi.NewEmail();

        var res = await client.PostAsJsonAsync("/api/auth/register", new { email, password = TestApi.Password });

        await res.EnsureStatusAsync(HttpStatusCode.OK);
        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal(email, auth!.Email);
        Assert.False(string.IsNullOrEmpty(auth.AccessToken));

        var cookie = Assert.Single(res.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("refreshToken=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_rejects_duplicate_email_and_weak_password()
    {
        var client = factory.CreateApiClient();
        var email  = TestApi.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = TestApi.Password });

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new { email, password = TestApi.Password });
        var weak      = await client.PostAsJsonAsync("/api/auth/register", new { email = TestApi.NewEmail(), password = "short" });

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
    }

    [Fact]
    public async Task Login_works_with_correct_password_only()
    {
        var client = factory.CreateApiClient();
        var email  = TestApi.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = TestApi.Password });

        var ok    = await client.PostAsJsonAsync("/api/auth/login", new { email, password = TestApi.Password });
        var wrong = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPass1" });

        await ok.EnsureStatusAsync(HttpStatusCode.OK);
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
    }

    [Fact]
    public async Task Access_token_grants_access_to_protected_endpoints()
    {
        var (client, _) = await factory.RegisterAsync();

        var res = await client.GetAsync("/api/questions");

        await res.EnsureStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_old_cookie_stops_working()
    {
        var client = factory.CreateApiClient(handleCookies: false);
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { email = TestApi.NewEmail(), password = TestApi.Password });
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponse>())!;
        var firstCookie = RefreshCookie(register);

        var refreshed = await RefreshWith(client, firstCookie);
        await refreshed.EnsureStatusAsync(HttpStatusCode.OK);
        var body = await refreshed.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal(auth.UserId, body!.UserId);
        var secondCookie = RefreshCookie(refreshed);
        Assert.NotEqual(firstCookie, secondCookie);

        // The rotated-out token can't be replayed, the new one still works
        Assert.Equal(HttpStatusCode.Unauthorized, (await RefreshWith(client, firstCookie)).StatusCode);
        await (await RefreshWith(client, secondCookie)).EnsureStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var (client, _) = await factory.RegisterAsync();

        var logout = await client.PostAsync("/api/auth/logout", null);
        await logout.EnsureStatusAsync(HttpStatusCode.NoContent);

        var refresh = await client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Tampered_token_is_rejected()
    {
        var (client, auth) = await factory.RegisterAsync();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken[..^4] + "abcd");

        var res = await client.GetAsync("/api/questions");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    private static string RefreshCookie(HttpResponseMessage res) =>
        res.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

    private static Task<HttpResponseMessage> RefreshWith(HttpClient client, string cookie)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        req.Headers.Add("Cookie", cookie);
        return client.SendAsync(req);
    }
}
