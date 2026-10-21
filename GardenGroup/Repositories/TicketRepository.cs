using GardenGroup.Models;
using MongoDB.Driver;

namespace GardenGroup.Repositories
{
    public class TicketRepository : ITicketRepository
    {

        private readonly IMongoCollection<Ticket> _tickets;

        public TicketRepository(IMongoDatabase database)
        {
            _tickets = database.GetCollection<Ticket>("tickets");
        }

        public void CreateTicket(Ticket ticket)
        {
            _tickets.InsertOne(ticket);
        }

        public void DeleteTicket(string id)
        {
            _tickets.DeleteOne(t => t.Id == id);
        }

        public List<Ticket> GetAllTickets()
        {
            return _tickets.Find(_ => true).ToList();
        }

        public Ticket? GetTicketById(string id)
        {
            return _tickets.Find(t => t.Id == id).FirstOrDefault();
        }

        public void UpdateTicket(Ticket ticket)
        {
            _tickets.ReplaceOne(t => t.Id == ticket.Id, ticket);
        }
    }
}
