using GardenGroup.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GardenGroup.Repositories;
namespace GardenGroup.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ITicketQueries _queries;
        public DashboardController(ITicketQueries queries) => _queries = queries;

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                Statuses = await _queries.GetStatusPercentagesAsync(null),
                Workloads = await _queries.GetEmployeeWorkloadsAsync()
            };
            return View(vm);
        }
    }
}
