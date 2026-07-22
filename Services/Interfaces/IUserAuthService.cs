using Fruitables.Models;
using Fruitables.ViewModels;

namespace Fruitables.Services.Interfaces;

/// <summary>
/// Interface for user authentication service (registration, login, logout)
/// </summary>
public interface IUserAuthService
{
    /// <summary>
    /// Register a new user account
    /// </summary>
    Task<RegistrationResult> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Login with email and password
    /// </summary>
    Task<AuthResult> LoginAsync(string email, string password);

    /// <summary>
    /// Logout current user
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Get redirect URL based on user role
    /// Customer -> /Home/Index
    /// Admin/SuperAdmin -> /Admin/Dashboard
    /// </summary>
    string GetRedirectUrlByRole(UserRole role);

    /// <summary>
    /// Hash password using BCrypt
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify password against BCrypt hash
    /// </summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Generate a password reset token and save to DB
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="phone">User's phone number</param>
    /// <returns>Token if email and phone match, null otherwise</returns>
    Task<string?> GeneratePasswordResetTokenAsync(string email, string phone);

    /// <summary>
    /// Validate token and reset user's password
    /// </summary>
    /// <param name="request">Reset password request containing email, token and new password</param>
    /// <returns>True if reset successful, false if token invalid/expired</returns>
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Change user's password
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}
