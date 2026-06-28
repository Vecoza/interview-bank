using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InterviewBank.API.Data;
using InterviewBank.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InterviewBank.API.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext   _db;

    public TokenService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db     = db;
    }

    // ── Access token (15 min, in-memory on client) ────────────────────────────

    public string GenerateAccessToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSecret"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier,      user.Id)
        };

        var token = new JwtSecurityToken(
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Refresh token (7 days, HttpOnly cookie on client) ────────────────────

    public async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        var rt = new RefreshToken
        {
            Id        = Guid.NewGuid(),
            Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId    = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();
        return rt;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token)
    {
        return await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token        == token &&
                t.RevokedAt    == null  &&
                t.ExpiresAt    >  DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Rotates the refresh token: revokes the old one, issues a new one.
    /// Returns both new tokens so the controller can write the cookie and JSON.
    /// </summary>
    public async Task<(string accessToken, RefreshToken refreshToken)>
        RotateTokensAsync(RefreshToken oldToken)
    {
        oldToken.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        var newRt     = await CreateRefreshTokenAsync(oldToken.UserId);
        var accessJwt = GenerateAccessToken(oldToken.User);
        return (accessJwt, newRt);
    }

    /// <summary>Revokes all active refresh tokens for a user (logout).</summary>
    public async Task RevokeAllForUserAsync(string userId)
    {
        var active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in active)
            t.RevokedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
    }
}
