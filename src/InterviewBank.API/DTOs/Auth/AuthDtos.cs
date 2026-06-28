using System.ComponentModel.DataAnnotations;

namespace InterviewBank.API.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
    public string UserId      { get; set; } = string.Empty;
}
