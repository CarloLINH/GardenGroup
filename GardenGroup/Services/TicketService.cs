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
        private readonly ITicketSearcher _searcher;

        public TicketService(
            ITicketRepository ticketRepository,
            ITicketPrioritySorter prioritySorter,
            ITicketSearcher searcher)
        {
            _ticketRepository = ticketRepository;
            _prioritySorter = prioritySorter;
            _searcher = searcher;
        }

        public Ticket? GetTicketById(string id)
        {
            return _ticketRepository.GetTicketById(id);
        }

        public TicketDetails? GetTicketWithEmployees(string id)
        {
            return _ticketRepository.GetTicketWithEmployees(id);
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

        public async Task<List<Ticket>> GetTicketsAsync(TicketListQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                return await _searcher.SearchAsync(query);

            }

            if (query.Sort is TicketSort.PriorityDesc or TicketSort.PriorityAsc)
            {
                var order = query.Sort == TicketSort.PriorityDesc
                     ? TicketSortOrder.PriorityHighToLow
                    : TicketSortOrder.PriorityLowToHigh;

                return await _prioritySorter.GetSortedAsync(
                 order, query.AssignedToId, query.ReportedById, query.Status, query.Page, query.PageSize);
            }

            return _ticketRepository.GetTickets(
            query.Status, query.AssignedToId, query.ReportedById, query.Sort != TicketSort.Oldest);
        }
    }
}
