using GardenGroup.Models;
using MongoDB.Driver;

namespace GardenGroup.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly IMongoCollection<Employee> _employees;

    public EmployeeRepository(IMongoDatabase database)
    {
        _employees = database.GetCollection<Employee>("employees");
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        return await _employees.Find(FilterDefinition<Employee>.Empty)
            .SortBy(employee => employee.EmployeeId)
            .ToListAsync();
    }

    public Task<Employee?> GetByIdAsync(string id)
    {
        return _employees.Find(employee => employee.Id == id).FirstOrDefaultAsync();
    }

    public Task<Employee?> GetWithHighestEmployeeIdAsync()
    {
        return _employees.Find(FilterDefinition<Employee>.Empty)
            .SortByDescending(employee => employee.EmployeeId)
            .FirstOrDefaultAsync();
    }

    public Task<bool> ExistsByEmployeeIdAsync(int employeeId, string? excludedId = null)
    {
        var filter = Builders<Employee>.Filter.Eq(employee => employee.EmployeeId, employeeId);
        return ExistsExcludingAsync(filter, excludedId);
    }

    public Task<bool> ExistsByEmailAsync(string email, string? excludedId = null)
    {
        var filter = Builders<Employee>.Filter.Eq(employee => employee.Email, email);
        return ExistsExcludingAsync(filter, excludedId);
    }

    public Task InsertAsync(Employee employee)
    {
        return _employees.InsertOneAsync(employee);
    }

    public Task ReplaceAsync(Employee employee)
    {
        return _employees.ReplaceOneAsync(employeeItem => employeeItem.Id == employee.Id, employee);
    }

    public Task DeactivateAsync(string id)
    {
        var update = Builders<Employee>.Update.Set(employee => employee.IsActive, false);
        return _employees.UpdateOneAsync(employee => employee.Id == id, update);
    }

    private async Task<bool> ExistsExcludingAsync(
        FilterDefinition<Employee> filter,
        string? excludedId)
    {
        if (!string.IsNullOrWhiteSpace(excludedId))
        {
            filter &= Builders<Employee>.Filter.Ne(employee => employee.Id, excludedId);
        }

        return await _employees.Find(filter).Limit(1).AnyAsync();
    }
}
