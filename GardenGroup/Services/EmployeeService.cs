using GardenGroup.Models;
using GardenGroup.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GardenGroup.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher<Employee> _passwordHasher;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IPasswordHasher<Employee> passwordHasher)
    {
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Employee?> AuthenticateAsync(string email, string password)
    {
        var employee = await _employeeRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
        if (employee is null || !employee.Active || string.IsNullOrWhiteSpace(employee.PasswordHash))
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(employee, employee.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded
            ? employee
            : null;
    }

    public Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        return _employeeRepository.GetAllAsync();
    }

    public List<Employee> GetActiveEmployees()
    {
        return _employeeRepository.GetActive();
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


    public async Task CreateAsync(Employee employee, string password)
    {
        if (await EmployeeIdExistsAsync(employee.EmployeeId))
        {
            throw new InvalidOperationException(
                "An employee with this employee ID already exists.");
        }

        if (await EmailExistsAsync(employee.Email))
        {
            throw new InvalidOperationException(
                "An employee with this email address already exists.");
        }

        employee.Email = employee.Email.Trim().ToLowerInvariant();
        employee.PasswordHash = _passwordHasher.HashPassword(employee, password);
        await _employeeRepository.InsertAsync(employee);
    }

    public async Task UpdateAsync(Employee employee, string? password)
    {

        if (string.IsNullOrWhiteSpace(employee.Id))
        {
            throw new ArgumentException(
                "The employee identifier is invalid.",
                nameof(employee));
        }

        if (!await ObjectIdExistsAsync(employee.Id))
        {
            throw new KeyNotFoundException("Employee was not found.");
        }

        if (await EmployeeIdExistsAsync(
            employee.EmployeeId,
            employee.Id))
        {
            throw new InvalidOperationException(
                "An employee with this employee ID already exists.");
        }

        employee.Email = employee.Email.Trim().ToLowerInvariant();
        if (await EmailExistsAsync(employee.Email, employee.Id))
        {
            throw new InvalidOperationException(
                "An employee with this email address already exists.");
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            employee.PasswordHash = _passwordHasher.HashPassword(employee, password);
            employee.PasswordResetTime = null;
            employee.PasswordResetLink = null;
        }

        await _employeeRepository.ReplaceAsync(employee);
    }

    public Task DeactivateAsync(string id)
    {
        return _employeeRepository.DeactivateAsync(id);
    }

    private async Task<bool> ObjectIdExistsAsync(string id)
    {
        return await _employeeRepository.GetByIdAsync(id) is not null;
    }
}
