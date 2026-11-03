// Author: Carlo
using GardenGroup.Enums;
using GardenGroup.Models;

namespace GardenGroup.Repositories;

public interface ITicketPrioritySorter
{
    /// <summary>Haalt één pagina tickets op, gesorteerd op prioriteit.</summary>
    /// <param name="reportedBy">Alleen tickets van deze melder. Null = alle tickets.</param>
    /// <param name="status">Null = alle statussen.</param>
    /// <param name="page">Nulgebaseerd paginanummer.</param>
    /// <param name="pageSize">Tickets per pagina (1 tot 100).</param>
    /// <returns>De tickets van deze pagina, of een lege lijst.</returns>
    Task<List<Ticket>> GetSortedAsync(TicketSortOrder order, int? reportedBy, Statuses? status, int page, int pageSize);

    /// <summary>Maakt de indexen voor de sortering aan.</summary>
    Task EnsureIndexesAsync();
}