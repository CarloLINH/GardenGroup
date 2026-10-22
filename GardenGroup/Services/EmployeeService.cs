using GardenGroup.Models;
using GardenGroup.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GardenGroup.Services;

public sealed class EmployeeService : IEmployeeService
{
    private static readonly IReadOnlyList<string> Roles =
        new[] { "manager", "admin", "agent", "employee" };

    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher<Employee> _passwordHasher;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IPasswordHasher<Employee> passwordHasher)
    {
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
    }

    public IReadOnlyList<string> AllowedRoles => Roles;

    public Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        return _employeeRepository.GetAllAsync();
    }

    public Task<Employee?> GetByIdAsync(string? id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? Task.FromResult<Employee?>(null)
            : _employeeRepository.GetByIdAsync(id);
    }

    public async Task<int> GetNextEmployeeIdAsync()
    {
        var employee = await _employeeRepository.GetWithHighestEmployeeIdAsync();
        return employee is null ? 1 : employee.EmployeeId + 1;
    }

    public Task<bool> EmployeeIdExistsAsync(int employeeId, string? excludedId = null)
    {
        return _employeeRepository.ExistsByEmployeeIdAsync(employeeId, excludedId);
    }

    public Task<bool> EmailExistsAsync(string email, string? excludedId = null)
    {
        return _employeeRepository.ExistsByEmailAsync(email, excludedId);
    }

    public bool IsValidRole(string role)
    {
        return Roles.Contains(role, StringComparer.Ordinal);
    }

    public async Task CreateAsync(Employee employee, string password)
    {
        EnsureValidRole(employee.Role);
        employee.EmployeeId = await GetNextEmployeeIdAsync();
        employee.Password = _passwordHasher.HashPassword(employee, password);
        await _employeeRepository.InsertAsync(employee);
    }

    public async Task UpdateAsync(Employee employee, string? password)
    {
        EnsureValidRole(employee.Role);

        if (!string.IsNullOrWhiteSpace(password))
        {
            employee.Password = _passwordHasher.HashPassword(employee, password);
            employee.PasswordResetTime = null;
            employee.PasswordResetLink = null;
        }

        await _employeeRepository.ReplaceAsync(employee);
    }

    public Task DeactivateAsync(string id)
    {
        return _employeeRepository.DeactivateAsync(id);
    }

    private void EnsureValidRole(string role)
    {
        if (!IsValidRole(role))
        {
            throw new ArgumentException("The selected role is not valid.", nameof(role));
        }
    }
}
