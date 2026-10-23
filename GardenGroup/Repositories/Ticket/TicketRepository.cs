using GardenGroup.Models;
using GardenGroup.Enums;
using GardenGroup.Models;
using GardenGroup.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
            ArgumentNullException.ThrowIfNull(ticket);

            if (ticket.TicketId <= 0)
            {
                throw new ArgumentException("TicketId must be greater than zero.", nameof(ticket));
            }

            if (ticket.ReportById <= 0)
            {
                throw new ArgumentException("ReportById must be greater than zero.", nameof(ticket));
            }

            _tickets.InsertOne(ticket);
        }

        public void DeleteTicket(string id)
        {
            try
            {
                ObjectId.Parse(id);
            }
            catch (FormatException)
            {
                return;
            }
            
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


        public TicketDetails? GetTicketWithEmployees(string id)
        {
            ObjectId objectId;

            try
            {
                objectId = ObjectId.Parse(id);
            }
            catch (FormatException)
            {
                return null;
            }

            var pipeline = new[]
            {
        new BsonDocument("$match",
            new BsonDocument("_id", objectId)),

        new BsonDocument("$lookup",
            new BsonDocument
            {
                { "from", "employees" },
                { "localField", "reportedBy" },
                { "foreignField", "employeeId" },
                { "as", "reporters" }
            }),

        new BsonDocument("$lookup",
            new BsonDocument
            {
                { "from", "employees" },
                { "localField", "assignedTo" },
                { "foreignField", "employeeId" },
                { "as", "assignees" }
            })
    };

            var document = _tickets
                .Aggregate<BsonDocument>(pipeline)
                .FirstOrDefault();

            if (document is null)
            {
                return null;
            }

            var ticket = BsonSerializer.Deserialize<Ticket>(document);

            return new TicketDetails
            {
                Id = ticket.Id,
                TicketId = ticket.TicketId,
                ReportById = ticket.ReportById,
                AssignedToId = ticket.AssignedToId,
                Title = ticket.Title,
                Description = ticket.Description,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Status = ticket.Status,
                Priority = ticket.Priority,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt,
                Reporter = GetFirstEmployee(document, "reporters"),
                Assignee = GetFirstEmployee(document, "assignees")
            };
        }

        private static Employee? GetFirstEmployee(
            BsonDocument document,
            string fieldName)
        {
            if (!document.TryGetValue(fieldName, out var value) ||
                !value.IsBsonArray ||
                value.AsBsonArray.Count == 0)
            {
                return null;
            }

            var employeeDocument = value.AsBsonArray[0];

            if (employeeDocument.IsBsonNull)
            {
                return null;
            }

            return BsonSerializer.Deserialize<Employee>(
                employeeDocument.AsBsonDocument);
        }

        public void UpdateTicket(Ticket ticket)
        {
            var filter = Builders<Ticket>.Filter.Eq(t => t.Id, ticket.Id);

            var update = Builders<Ticket>.Update
                .Set(t => t.Title, ticket.Title)
                .Set(t => t.Description, ticket.Description)
                .Set(t => t.AssignedToId, ticket.AssignedToId)
                .Set(t => t.Priority, ticket.Priority)
                .Set(t => t.Status, ticket.Status)
                .Set(t => t.UpdatedAt, DateTime.Now);

            if (ticket.Status == Statuses.Resolved && ticket.ResolvedAt == null)
            {
                update = update.Set(t => t.ResolvedAt, DateTime.Now);
            }
            if (ticket.Status == Statuses.Closed && ticket.ClosedAt == null)
            {
                update = update.Set(t => t.ClosedAt, DateTime.Now);
            }

            _tickets.UpdateOne(filter, update);
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
