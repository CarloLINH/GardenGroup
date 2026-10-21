using GardenGroup.Models;

namespace GardenGroup.Repositories
{
    public interface ITicketRepository
    {
        List<Ticket> GetAllTickets();

        Ticket? GetTicketById(string id);

        void CreateTicket(Ticket ticket);

        void UpdateTicket(Ticket ticket);

        void DeleteTicket(string id);
    }
}
