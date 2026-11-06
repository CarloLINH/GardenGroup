namespace GardenGroup.Services;

public interface IForgotPasswordService
{
    Task<string?> CreateResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
}