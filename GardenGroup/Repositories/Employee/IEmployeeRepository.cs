using GardenGroup.Models;

namespace GardenGroup.Repositories;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(string id);
    Task<Employee?> GetWithHighestEmployeeIdAsync();
    Task<bool> ExistsByEmployeeIdAsync(int employeeId, string? excludedId = null);
    Task<bool> ExistsByEmailAsync(string email, string? excludedId = null);
    Task InsertAsync(Employee employee);
    Task ReplaceAsync(Employee employee);
    Task DeactivateAsync(string id);
}
