using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.ViewModels;

namespace GardenGroup.Repositories
{
    public interface ITicketRepository
    {

        Ticket? GetTicketById(string id);
        TicketDetails? GetTicketWithEmployees(string id);

        void CreateTicket(Ticket ticket);

        void UpdateTicket(Ticket ticket);

        void DeleteTicket(string id);

        int GetLastTicketId();

        List<Ticket> GetTickets(Statuses? status, int? assignedToId, int? reportedById, bool newestFirst = true);
    }
}
