using GardenGroup.Models;
using MongoDB.Driver;
using GardenGroup.Enums;

namespace GardenGroup.Repositories
{
    public class TicketRepository : ITicketRepository
    {

        private readonly IMongoCollection<Ticket> _tickets;

        public TicketRepository(IMongoDatabase database)
        {
            _tickets = database.GetCollection<Ticket>("tickets");
            CreateIndexes();
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

        public List<Ticket> GetTicketsByStatus(Statuses status)
        {
            return _tickets.Find(t => t.Status == status).ToList();
        }

        public List<Ticket> GetTicketsByAssignedEmployee(int employeeId)
        {
            return _tickets.Find(t => t.AssignedToId == employeeId).ToList();
        }

        public List<Ticket> GetTicketsSortedByCreationDate(bool newestFirst)
        {
            if (newestFirst)
            {
                return _tickets.Find(_ => true).SortByDescending(t => t.CreatedAt).ToList();
            }
            else
            {
                return _tickets.Find(_ => true).SortBy(t => t.CreatedAt).ToList();
            }
        }

        private void CreateIndexes()
        {
            var indexes = new[]
            {
                new CreateIndexModel<Ticket>(Builders<Ticket>.IndexKeys.Ascending(t => t.Status)),

                new CreateIndexModel<Ticket>(Builders<Ticket>.IndexKeys.Ascending(t => t.AssignedToId)),

                new CreateIndexModel<Ticket>(Builders<Ticket>.IndexKeys.Descending(t => t.CreatedAt))
             };
            _tickets.Indexes.CreateMany(indexes);
        }

    }
}
