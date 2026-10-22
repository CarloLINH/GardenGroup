using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.ViewModels;

namespace GardenGroup.Repositories
{
    public interface ITicketRepository
    {
        List<Ticket> GetAllTickets();

        Ticket? GetTicketById(string id);
        TicketDetails? GetTicketWithEmployees(string id);

        void CreateTicket(Ticket ticket);

        void UpdateTicket(Ticket ticket);

        void DeleteTicket(string id);

        List<Ticket> GetTicketsByStatus(Statuses status);
        List<Ticket> GetTicketsByAssignedEmployee(int employeeId);
        List<Ticket> GetTicketsSortedByCreationDate(bool newestFirst);

        List<Ticket> GetTickets(Statuses? status, int? assignedToId, bool newestFirst = true);
        int GetLastTicketId();

    }
}
