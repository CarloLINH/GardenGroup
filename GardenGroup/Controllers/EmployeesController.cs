using GardenGroup.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace GardenGroup.Controllers;

public sealed class EmployeesController : Controller
{
    private readonly IMongoCollection<Employee> _employees;
    private readonly IPasswordHasher<Employee> _passwordHasher = new PasswordHasher<Employee>();

    public EmployeesController(IMongoCollection<Employee> employees)
    {
        _employees = employees;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employees.Find(FilterDefinition<Employee>.Empty)
            .SortBy(employee => employee.EmployeeId)
            .ToListAsync();

        return View(employees);
    }

    public async Task<IActionResult> Details(string? id)
    {
        var employee = await FindEmployeeAsync(id);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EmployeeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        model.FirstName = model.FirstName.Trim();
        model.LastName = model.LastName.Trim();
        model.Email = model.Email.Trim();
        model.Role = model.Role.Trim();

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "A password is required for a new employee.");
        }

        if (await EmployeeIdExistsAsync(model.EmployeeId) && ModelState.IsValid)
        {
            ModelState.AddModelError(nameof(model.EmployeeId), "This employee ID is already in use.");
        }

        if (await EmailExistsAsync(model.Email) && ModelState.IsValid)
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var employee = new Employee
        {
            EmployeeId = model.EmployeeId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Role = model.Role,
            IsActive = true
        };
        employee.Password = _passwordHasher.HashPassword(employee, model.Password!);

        await _employees.InsertOneAsync(employee);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        var employee = await FindEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return View(new EmployeeFormViewModel
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Role = employee.Role,
            IsActive = employee.Active
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, EmployeeFormViewModel model)
    {
        var employee = await FindEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        model.FirstName = model.FirstName.Trim();
        model.LastName = model.LastName.Trim();
        model.Email = model.Email.Trim();
        model.Role = model.Role.Trim();

        if (await EmployeeIdExistsAsync(model.EmployeeId, id) && ModelState.IsValid)
        {
            ModelState.AddModelError(nameof(model.EmployeeId), "This employee ID is already in use.");
        }

        if (await EmailExistsAsync(model.Email, id) && ModelState.IsValid)
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        employee.EmployeeId = model.EmployeeId;
        employee.FirstName = model.FirstName;
        employee.LastName = model.LastName;
        employee.Email = model.Email;
        employee.Role = model.Role;
        employee.IsActive = model.IsActive;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            employee.Password = _passwordHasher.HashPassword(employee, model.Password);
            employee.PasswordResetTime = null;
            employee.PasswordResetLink = null;
        }

        await _employees.ReplaceOneAsync(employeeItem => employeeItem.Id == id, employee);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Deactivate(string? id)
    {
        var employee = await FindEmployeeAsync(id);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost, ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(string? id)
    {
        var employee = await FindEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        var update = Builders<Employee>.Update.Set(employeeItem => employeeItem.IsActive, false);
        await _employees.UpdateOneAsync(employeeItem => employeeItem.Id == id, update);

        return RedirectToAction(nameof(Index));
    }

    private Task<Employee?> FindEmployeeAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Task.FromResult<Employee?>(null);
        }

        return _employees.Find(employee => employee.Id == id).FirstOrDefaultAsync();
    }

    private Task<bool> EmployeeIdExistsAsync(int employeeId, string? excludedId = null)
    {
        var filter = Builders<Employee>.Filter.Eq(employee => employee.EmployeeId, employeeId);
        return ExistsExcludingAsync(filter, excludedId);
    }

    private Task<bool> EmailExistsAsync(string email, string? excludedId = null)
    {
        var filter = Builders<Employee>.Filter.Eq(employee => employee.Email, email);
        return ExistsExcludingAsync(filter, excludedId);
    }

    private async Task<bool> ExistsExcludingAsync(FilterDefinition<Employee> filter, string? excludedId)
    {
        if (!string.IsNullOrWhiteSpace(excludedId))
        {
            filter &= Builders<Employee>.Filter.Ne(employee => employee.Id, excludedId);
        }

        return await _employees.Find(filter).Limit(1).AnyAsync();
    }
}
