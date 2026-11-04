using GardenGroup.Models;
using GardenGroup.Repositories;
using GardenGroup.Services;
using Microsoft.AspNetCore.Identity;

namespace GardenGroup.Tests;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task Inactive_employee_cannot_authenticate()
    {
        var hasher = new PasswordHasher<Employee>();
        var employee = new Employee
        {
            EmployeeId = 1,
            Email = "inactive@example.com",
            PasswordHash = "",
            Role = "regular-employee",
            IsActive = false
        };
        employee.PasswordHash = hasher.HashPassword(employee, "correct-password");
        var service = new EmployeeService(new InMemoryEmployeeRepository(employee), hasher);

        var result = await service.AuthenticateAsync(employee.Email, "correct-password");

        Assert.Null(result);
    }

    [Fact]
    public async Task Reset_token_can_be_used_once()
    {
        var hasher = new PasswordHasher<Employee>();
        var employee = new Employee
        {
            EmployeeId = 1,
            Email = "user@example.com",
            Role = "regular-employee",
            IsActive = true,
            PasswordHash = hasher.HashPassword(new Employee(), "old-password")
        };
        var repository = new InMemoryEmployeeRepository(employee);
        var service = new ForgotPasswordService(repository, hasher);

        var token = await service.CreateResetTokenAsync(employee.Email);
        Assert.NotNull(token);
        Assert.True(await service.ResetPasswordAsync(employee.Email, token!, "new-password"));
        Assert.False(await service.ResetPasswordAsync(employee.Email, token!, "another-password"));
    }

    private sealed class InMemoryEmployeeRepository : IEmployeeRepository
    {
        private Employee _employee;

        public InMemoryEmployeeRepository(Employee employee) => _employee = employee;
        public Task<IReadOnlyList<Employee>> GetAllAsync() => Task.FromResult<IReadOnlyList<Employee>>(new[] { _employee });
        public Task<Employee?> GetByIdAsync(string id) => Task.FromResult<Employee?>(_employee.Id == id ? _employee : null);
        public Task<Employee?> GetByEmailAsync(string email) => Task.FromResult<Employee?>(_employee.Email == email ? _employee : null);
        public Task<Employee?> GetWithHighestEmployeeIdAsync() => Task.FromResult<Employee?>(_employee);
        public Task<bool> ExistsByEmployeeIdAsync(int employeeId, string? excludedId = null) => Task.FromResult(_employee.EmployeeId == employeeId && _employee.Id != excludedId);
        public Task<bool> ExistsByEmailAsync(string email, string? excludedId = null) => Task.FromResult(_employee.Email == email && _employee.Id != excludedId);
        public Task InsertAsync(Employee employee) { _employee = employee; return Task.CompletedTask; }
        public Task ReplaceAsync(Employee employee) { _employee = employee; return Task.CompletedTask; }
        public Task DeactivateAsync(string id) { _employee.IsActive = false; return Task.CompletedTask; }
    }
}
