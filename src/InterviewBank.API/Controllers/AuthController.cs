using InterviewBank.API.DTOs.Auth;
using InterviewBank.API.Entities;
using InterviewBank.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InterviewBank.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";

    private readonly UserManager<AppUser> _users;
    private readonly TokenService         _tokens;

    public AuthController(UserManager<AppUser> users, TokenService tokens)
    {
        _users  = users;
        _tokens = tokens;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new AppUser { UserName = dto.Email, Email = dto.Email };
        var result = await _users.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return await IssueTokenPair(user);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { error = "Invalid credentials." });

        return await IssueTokenPair(user);
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var cookie = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(cookie))
            return Unauthorized(new { error = "No refresh token." });

        var stored = await _tokens.GetActiveRefreshTokenAsync(cookie);
        if (stored is null)
            return Unauthorized(new { error = "Refresh token invalid or expired." });

        var (accessJwt, newRt) = await _tokens.RotateTokensAsync(stored);
        SetRefreshCookie(newRt.Token, newRt.ExpiresAt);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessJwt,
            Email       = stored.User.Email!,
            UserId      = stored.UserId
        });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var cookie = Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(cookie))
        {
            var stored = await _tokens.GetActiveRefreshTokenAsync(cookie);
            if (stored is not null)
                await _tokens.RevokeAllForUserAsync(stored.UserId);
        }

        Response.Cookies.Delete(RefreshTokenCookie);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<IActionResult> IssueTokenPair(AppUser user)
    {
        var accessJwt = _tokens.GenerateAccessToken(user);
        var rt        = await _tokens.CreateRefreshTokenAsync(user.Id);

        SetRefreshCookie(rt.Token, rt.ExpiresAt);

        return Ok(new AuthResponseDto
        {
            AccessToken = accessJwt,
            Email       = user.Email!,
            UserId      = user.Id
        });
    }

    private void SetRefreshCookie(string token, DateTimeOffset expires)
    {
        Response.Cookies.Append(RefreshTokenCookie, token, new CookieOptions
        {
            HttpOnly  = true,
            Secure    = true,
            SameSite  = SameSiteMode.Strict,
            Expires   = expires
        });
    }
}
