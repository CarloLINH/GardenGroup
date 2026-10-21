using GardenGroup.Models;
namespace GardenGroup.ViewModels
{
    public class DashboardViewModel
    {
        public List<StatusPercentage> Statuses { get; set; } = new();
        public List<EmployeeWorkload> Workloads { get; set; } = new();
    }
}
