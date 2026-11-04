using GardenGroup.Models;

namespace GardenGroup.Services;

public interface IEmployeeService
{

    Task<IReadOnlyList<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(string? id);
    Task<Employee?> AuthenticateAsync(string email, string password);
    Task<int> GetNextEmployeeIdAsync();
    Task<bool> EmployeeIdExistsAsync(int employeeId, string? excludedId = null);
    Task<bool> EmailExistsAsync(string email, string? excludedId = null);
    Task CreateAsync(Employee employee, string password);
    Task UpdateAsync(Employee employee, string? password);
    Task DeactivateAsync(string id);
}
