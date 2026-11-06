// Author: Mart
using GardenGroup.Models;

namespace GardenGroup.Repositories;

//Individual functionality (Mart): search tickets on words in the title and the description.
public interface ITicketSearcher
{
    Task<List<Ticket>> SearchAsync(TicketListQuery query);
}