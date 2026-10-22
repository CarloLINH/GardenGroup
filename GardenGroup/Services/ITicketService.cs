using GardenGroup.Enums;
using GardenGroup.Models;

namespace GardenGroup.Services
{
    public interface ITicketService
    {
        List<Ticket> GetAllTickets();
        Ticket GetTicketById(string id);
        void CreateTicket(Ticket ticket);
        void UpdateTicket(Ticket ticket);
        void DeleteTicket(string id);
        List<Ticket> GetTickets(Statuses? status, int? assignedToId, bool newestFirst);
    }
}
