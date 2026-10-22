using GardenGroup.Models;
using GardenGroup.Services;
using Microsoft.AspNetCore.Mvc;

namespace GardenGroup.Controllers;

public sealed class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _employeeService.GetAllAsync());
    }

    public async Task<IActionResult> Details(string? id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new EmployeeFormViewModel
        {
            EmployeeId = await _employeeService.GetNextEmployeeIdAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        model.FirstName = model.FirstName.Trim();
        model.LastName = model.LastName.Trim();
        model.Email = model.Email.Trim();
        model.Role = model.Role.Trim();

        if (!_employeeService.IsValidRole(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Please select a valid role.");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "A password is required for a new employee.");
        }

        if (await _employeeService.EmailExistsAsync(model.Email) && ModelState.IsValid)
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var employee = new Employee
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Role = model.Role,
            IsActive = true
        };
        await _employeeService.CreateAsync(employee, model.Password!);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
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
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        model.EmployeeId = employee.EmployeeId;
        model.FirstName = model.FirstName.Trim();
        model.LastName = model.LastName.Trim();
        model.Email = model.Email.Trim();
        model.Role = model.Role.Trim();

        if (!_employeeService.IsValidRole(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Please select a valid role.");
        }

        if (await _employeeService.EmailExistsAsync(model.Email, id) && ModelState.IsValid)
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

        await _employeeService.UpdateAsync(employee, model.Password);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Deactivate(string? id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        return employee is null ? NotFound() : View(employee);
    }

    [HttpPost, ActionName("Deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(string? id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        await _employeeService.DeactivateAsync(id!);

        return RedirectToAction(nameof(Index));
    }

}
