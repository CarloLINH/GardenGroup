using GardenGroup.Models;
using GardenGroup.Enums;
using GardenGroup.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GardenGroup.Repositories
{
    public class TicketRepository : ITicketRepository
    {

        private readonly IMongoCollection<Ticket> _tickets;
        private readonly IMongoCollection<Employee> _employees;

        public TicketRepository(IMongoDatabase database)
        {
            _tickets = database.GetCollection<Ticket>("tickets");
            _employees = database.GetCollection<Employee>("employees");
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
                Type = ticket.Type,
                Deadline = ticket.Deadline,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt,
                EscalatedAt = ticket.EscalatedAt,
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
                .Set(t => t.Type, ticket.Type)
                .Set(t => t.Deadline, ticket.Deadline)
                .Set(t => t.Status, ticket.Status)
                .Set(t => t.UpdatedAt, DateTime.Now);

            var current = _tickets.Find(t => t.Id == ticket.Id).FirstOrDefault();
            if (current is not null && current.Status != ticket.Status)
            {
                if (ticket.Status == Statuses.Resolved) update = update.Set(t => t.ResolvedAt, DateTime.Now);
                if (ticket.Status == Statuses.Closed) update = update.Set(t => t.ClosedAt, DateTime.Now);
                if (ticket.Status == Statuses.Escalated) update = update.Set(t => t.EscalatedAt, DateTime.Now);
            }

            _tickets.UpdateOne(filter, update);
        }

        public bool UpdateStatus(string id, Statuses expectedCurrent, Statuses newStatus)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return false;
            }

            var update = Builders<Ticket>.Update
                .Set(t => t.Status, newStatus)
                .Set(t => t.UpdatedAt, DateTime.Now);

            if (newStatus == Statuses.Closed) update = update.Set(t => t.ClosedAt, DateTime.Now);
            if (newStatus == Statuses.Escalated) update = update.Set(t => t.EscalatedAt, DateTime.Now);

            var result = _tickets.UpdateOne(t => t.Id == id && t.Status == expectedCurrent, update);
            return result.ModifiedCount == 1;
        }

        public List<Ticket> GetTickets(Statuses? status, int? assignedToId, int? reportedById, bool newestFirst = true)
        {
            var filter = TicketFilterBuilder.Build(status, assignedToId, reportedById);

            IFindFluent<Ticket, Ticket> find = _tickets.Find(filter).Project<Ticket>(Builders<Ticket>.Projection.Exclude(t => t.Description));
            find = newestFirst ? find.SortByDescending(t => t.CreatedAt) : find.SortBy(t => t.CreatedAt);
            return find.ToList();
        }

        public int GetLastTicketId() =>
            _tickets.Find(_ => true)
                .SortByDescending(t => t.TicketId)
                .Limit(1)
                .Project(t => t.TicketId)
                .FirstOrDefault();

        public UpdateResult TransferTicket(string ticketId, int employeeId)
        {
            if (!ObjectId.TryParse(ticketId, out _))
            {
                throw new ArgumentException(
                    "The ticket identifier is invalid.",
                    nameof(ticketId));
            }

            if (employeeId <= 0)
            {
                throw new ArgumentException(
                    "The employee identifier is invalid.",
                    nameof(employeeId));
            }

            var filter = Builders<Ticket>.Filter.And(
                Builders<Ticket>.Filter.Eq(ticket => ticket.Id, ticketId),
                Builders<Ticket>.Filter.Ne(
                    ticket => ticket.AssignedToId,
                    employeeId));

            var update = Builders<Ticket>.Update
                .Set(ticket => ticket.AssignedToId, employeeId)
                .Set(ticket => ticket.UpdatedAt, DateTime.UtcNow);

            return _tickets.UpdateOne(filter, update);
        }
    }
}
