namespace EcommerceCA.Application.DTOs.Auth;

// ── Requests ──────────────────────────────────────────────────────────────────
public class LoginRequestDto
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    public string FirstName   { get; set; } = string.Empty;
    public string LastName    { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
    public string Password    { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Email       { get; set; } = string.Empty;
    public string Token       { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword     { get; set; } = string.Empty;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

// ── Responses ─────────────────────────────────────────────────────────────────
public class AuthResponseDto
{
    public string      AccessToken  { get; set; } = string.Empty;
    public string      RefreshToken { get; set; } = string.Empty;
    public DateTime    ExpiresAt    { get; set; }
    public UserInfoDto User         { get; set; } = null!;
}

public class UserInfoDto
{
    public string       Id             { get; set; } = string.Empty;
    public string       FirstName      { get; set; } = string.Empty;
    public string       LastName       { get; set; } = string.Empty;
    public string       FullName       => $"{FirstName} {LastName}".Trim();
    public string       Email          { get; set; } = string.Empty;
    public string?      PhoneNumber    { get; set; }
    public string?      ProfileImageUrl{ get; set; }
    public IList<string> Roles         { get; set; } = new List<string>();
}
