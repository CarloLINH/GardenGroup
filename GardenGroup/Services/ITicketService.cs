using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.ViewModels;

namespace GardenGroup.Services
{
    public interface ITicketService
    {
        Ticket? GetTicketById(string id);
        TicketDetails? GetTicketWithEmployees(string id);
        void CreateTicket(Ticket ticket);
        void UpdateTicket(Ticket ticket);
        void DeleteTicket(string id);
        Task<List<Ticket>> GetTicketsAsync(TicketListQuery query);
    }
}
