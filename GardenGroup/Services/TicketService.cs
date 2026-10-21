using GardenGroup.Models;
using GardenGroup.Repositories;

namespace GardenGroup.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public List<Ticket> GetAllTickets()
        {
            return _ticketRepository.GetAllTickets();
        }

        public Ticket GetTicketById(int id)
        {
            return _ticketRepository.GetTicketById(id);
        }

        public void CreateTicket(Ticket ticket)
        {
           
            var allTickets = _ticketRepository.GetAllTickets();
            var lastTicket = allTickets.OrderByDescending(t => t.TicketId).FirstOrDefault();

            ticket.TicketId = (lastTicket == null) ? 1 : lastTicket.TicketId + 1;

            ticket.CreatedAt = DateTime.Now;
            _ticketRepository.CreateTicket(ticket);
        }

        public void UpdateTicket(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.Now;
            _ticketRepository.UpdateTicket(ticket);
        }

        public void DeleteTicket(int id)
        {
            _ticketRepository.DeleteTicket(id);
        }
    }
}
