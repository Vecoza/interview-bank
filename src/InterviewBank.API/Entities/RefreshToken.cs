namespace InterviewBank.API.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    /// <summary>Cryptographically random base64 string stored server-side.</summary>
    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Set when the token is rotated or the user logs out.</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    // Convenience helpers — not mapped to columns
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive  => !IsExpired && !IsRevoked;

    public AppUser User { get; set; } = null!;
}
