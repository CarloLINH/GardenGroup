using GardenGroup.Models;

namespace GardenGroup.ViewModels;

/// <summary>Gegevens voor de dashboardpagina.</summary>
public class DashboardViewModel
{
    /// <summary>True: dashboard van één medewerker (regular). False: alle tickets (service desk).</summary>
    public bool IsPersonal { get; set; }

    /// <summary>Altijd drie categorieën in vaste volgorde.</summary>
    public List<StatusPercentage> Categories { get; set; } = new();

    /// <summary>Werkverdeling per medewerker. Alleen gevuld voor de service desk.</summary>
    public List<EmployeeWorkload> Workloads { get; set; } = new();

    /// <summary>Totaal aantal tickets in dit dashboard.</summary>
    public int Total => Categories.Sum(c => c.Count);
}
