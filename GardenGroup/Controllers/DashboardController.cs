using GardenGroup.Repositories;
using GardenGroup.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GardenGroup.Controllers;

public class DashboardController : Controller
{
    private readonly ITicketQueries _queries;

    public DashboardController(ITicketQueries queries) => _queries = queries;

    /// <summary>
    /// Toont het dashboard. Met userId: alleen de tickets van die medewerker (regular employee).
    /// Zonder userId: alle tickets plus de werkverdeling (service desk).
    /// </summary>
    /// <param name="userId">TIJDELIJK: vervang door de ingelogde gebruiker zodra login klaar is.</param>
    public async Task<IActionResult> Index(int? userId)
    {
        // TODO (na login): niet uit de querystring halen, anders kan iedereen andermans dashboard openen.
        //   Regular employee  -> userId = id van de ingelogde gebruiker (uit de claims)
        //   Service desk      -> userId = null
        //   Plus [Authorize] op deze controller.
        var vm = new DashboardViewModel
        {
            IsPersonal = userId.HasValue,
            Categories = await _queries.GetStatusPercentagesAsync(userId),
            Workloads = userId.HasValue ? new() : await _queries.GetEmployeeWorkloadsAsync()
        };
        return View(vm);
    }
}
