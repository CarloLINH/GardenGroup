using GardenGroup.Models;

namespace GardenGroup.Services;

public interface IEmployeeService
{
    IReadOnlyList<string> AllowedRoles { get; }
    Task<IReadOnlyList<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(string? id);
    Task<int> GetNextEmployeeIdAsync();
    Task<bool> EmployeeIdExistsAsync(int employeeId, string? excludedId = null);
    Task<bool> EmailExistsAsync(string email, string? excludedId = null);
    bool IsValidRole(string role);
    Task CreateAsync(Employee employee, string password);
    Task UpdateAsync(Employee employee, string? password);
    Task DeactivateAsync(string id);
}
