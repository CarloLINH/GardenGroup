// Author: Carlo (dashboard), Emre (login and role check)
using GardenGroup.Extensions;
using GardenGroup.Repositories;
using GardenGroup.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GardenGroup.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ITicketQueries _queries;

    public DashboardController(ITicketQueries queries) => _queries = queries;

    public async Task<IActionResult> Index()
    {
        int? userId = null;
        if (!User.IsServiceDesk())
        {
            userId = User.GetEmployeeId();
            if (userId is null)
            {
                return Forbid();
            }
        }

        var vm = new DashboardViewModel
        {
            IsPersonal = userId.HasValue,
            Categories = await _queries.GetStatusPercentagesAsync(userId),
            Workloads = userId.HasValue ? new() : await _queries.GetEmployeeWorkloadsAsync()
        };
        return View(vm);
    }
}
