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

        // Author: Emre. Changes the status only if the ticket still has expectedCurrent. False = nothing changed.
        bool UpdateStatus(string id, Statuses expectedCurrent, Statuses newStatus);

        void DeleteTicket(string id);

        int GetLastTicketId();

        List<Ticket> GetTickets(Statuses? status, int? assignedToId, int? reportedById, bool newestFirst = true);
    }
}
