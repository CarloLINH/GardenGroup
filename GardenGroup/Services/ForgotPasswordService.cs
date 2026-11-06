using System.Security.Cryptography;
using GardenGroup.Models;
using GardenGroup.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GardenGroup.Services;

public sealed class ForgotPasswordService : IForgotPasswordService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher<Employee> _passwordHasher;

    public ForgotPasswordService(
        IEmployeeRepository employeeRepository,
        IPasswordHasher<Employee> passwordHasher)
    {
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<string?> CreateResetTokenAsync(string email)
    {
        var employee = await _employeeRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
        if (employee is null || !employee.Active)
        {
            return null;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        employee.PasswordResetLink = token;
        employee.PasswordResetTime = DateTime.UtcNow.Add(TokenLifetime);
        await _employeeRepository.ReplaceAsync(employee);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var employee = await _employeeRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
        if (employee is null || !employee.Active || employee.PasswordResetTime is null ||
            employee.PasswordResetTime <= DateTime.UtcNow || !TokensMatch(employee.PasswordResetLink, token))
        {
            return false;
        }

        employee.PasswordHash = _passwordHasher.HashPassword(employee, newPassword);
        employee.PasswordResetLink = null;
        employee.PasswordResetTime = null;
        await _employeeRepository.ReplaceAsync(employee);
        return true;
    }

    private static string ToBase64(string value)
    {
        return value.Replace('-', '+').Replace('_', '/') + new string('=', (4 - value.Length % 4) % 4);
    }

    private static bool TokensMatch(string? storedToken, string token)
    {
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(ToBase64(storedToken ?? string.Empty)),
                Convert.FromBase64String(ToBase64(token)));
        }
        catch (FormatException)
        {
            return false;
        }
    }
}