using GardenGroup.Models;

namespace GardenGroup.Services
{
    public interface ITicketService
    {
        List<Ticket> GetAllTickets();
        Ticket GetTicketById(int id);
        void CreateTicket(Ticket ticket);
        void UpdateTicket(Ticket ticket);
        void DeleteTicket(int id);

    }
}
