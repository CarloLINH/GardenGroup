using GardenGroup.Models;
using GardenGroup.Enums;
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

        //Indexes for performance optimization
        public List<Ticket> GetTickets(Statuses? status, int? assignedToId, bool newestFirst = true)
        {
            var fb = Builders<Ticket>.Filter;
            var filter = fb.Empty;

            if (status.HasValue) filter &= fb.Eq(t => t.Status, status.Value);
            if (assignedToId.HasValue) filter &= fb.Eq(t => t.AssignedToId, assignedToId.Value);

            IFindFluent<Ticket, Ticket> find = _tickets.Find(filter).Project<Ticket>(Builders<Ticket>.Projection.Exclude(t => t.Description));
            find = newestFirst ? find.SortByDescending(t => t.CreatedAt) : find.SortBy(t => t.CreatedAt);
            return find.ToList();
        }

        public List<Ticket> GetTicketsByStatus(Statuses status) => GetTickets(status, null, true);

        public List<Ticket> GetTicketsByAssignedEmployee(int employeeId) => GetTickets(null, employeeId, true);

        public List<Ticket> GetTicketsSortedByCreationDate(bool newestFirst) => GetTickets(null, null, newestFirst);

        // Highest ticketId via the unique index (reverse index scan, 1 document).
        public int GetLastTicketId() =>
            _tickets.Find(_ => true)
                .SortByDescending(t => t.TicketId)
                .Limit(1)
                .Project(t => t.TicketId)
                .FirstOrDefault();
    }
}
