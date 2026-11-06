using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.Repositories;
using GardenGroup.ViewModels;
using MongoDB.Driver;

namespace GardenGroup.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketPrioritySorter _prioritySorter;

        public TicketService(
            ITicketRepository ticketRepository,
            ITicketPrioritySorter prioritySorter)
        {
            _ticketRepository = ticketRepository;
            _prioritySorter = prioritySorter;
        }

        public List<Ticket> GetAllTickets()
        {
            return _ticketRepository.GetAllTickets();
        }

        public Ticket? GetTicketById(string id)
        {
            return _ticketRepository.GetTicketById(id);
        }

        public TicketDetails? GetTicketWithEmployees(string id)
        {
            return _ticketRepository.GetTicketWithEmployees(id);
        }

        public List<Ticket> GetTickets(Statuses? status, int? assignedToId, bool newestFirst)
        {
            return _ticketRepository.GetTickets(status, assignedToId, newestFirst);
        }

        public void CreateTicket(Ticket ticket)
        {
            ticket.CreatedAt = DateTime.Now;

            for (var attempt = 0; attempt < 3; attempt++)
            {
                ticket.TicketId = _ticketRepository.GetLastTicketId() + 1;
                try
                {
                    _ticketRepository.CreateTicket(ticket);
                    return;
                }
                catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    ticket.Id = null;
                }
            }
            throw new InvalidOperationException("Could not generate a unique ticket id.");
        }

        public void UpdateTicket(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.Now;
            _ticketRepository.UpdateTicket(ticket);
        }

        public void DeleteTicket(string id)
        {
            _ticketRepository.DeleteTicket(id);
        }

        public async Task<List<Ticket>> GetTicketsAsync(Statuses? status, int? assignedToId, string sort, int page, int pageSize)
        {
            page = Math.Max(page, 0);

            if (sort is "priorityDesc" or "priorityAsc")
            {
                var order = sort == "priorityDesc"
                    ? TicketSortOrder.PriorityHighToLow
                    : TicketSortOrder.PriorityLowToHigh;

                return await _prioritySorter.GetSortedAsync(
                    order, assignedToId, status, page, pageSize);
            }

            return _ticketRepository.GetTickets(
                status, assignedToId, sort != "oldest");
        }
    }
}
