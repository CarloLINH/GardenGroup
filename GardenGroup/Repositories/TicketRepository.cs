using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GardenGroup.Repositories
{
   
        public class TicketRepository : ITicketRepository
        {
            private readonly IMongoCollection<Ticket> _tickets;
            public TicketRepository(IMongoDatabase db) => _tickets = db.GetCollection<Ticket>("tickets");

            public async Task<Ticket> CreateAsync(Ticket t)
            {
                Validate(t);
                t.Id = ObjectId.GenerateNewId();
                t.Status = TicketStatus.Open;
                t.CreatedAt = t.UpdatedAt = DateTime.UtcNow;
                t.ResolvedAt = t.ClosedAt = null;
                await _tickets.InsertOneAsync(t);
                return t;
            }

            public async Task<Ticket?> GetByIdAsync(string id) =>
                ObjectId.TryParse(id, out var oid)
                    ? await _tickets.Find(x => x.Id == oid).FirstOrDefaultAsync()
                    : null;

            public async Task<bool> UpdateAsync(Ticket t)
            {
                Validate(t);
                var now = DateTime.UtcNow;
                var update = Builders<Ticket>.Update
                    .Set(x => x.Title, t.Title).Set(x => x.Description, t.Description)
                    .Set(x => x.DateOfEvent, t.DateOfEvent).Set(x => x.Priority, t.Priority)
                    .Set(x => x.AssignedTo, t.AssignedTo).Set(x => x.UpdatedAt, now);
                update = Builders<Ticket>.Update.Combine(update, StatusFields(t.Status, now));
                return (await _tickets.UpdateOneAsync(x => x.Id == t.Id, update)).MatchedCount == 1;
            }

            public async Task<bool> UpdateStatusAsync(ObjectId id, TicketStatus newStatus)
            {
                var now = DateTime.UtcNow;
                var update = Builders<Ticket>.Update.Combine(
                    StatusFields(newStatus, now), Builders<Ticket>.Update.Set(x => x.UpdatedAt, now));
                return (await _tickets.UpdateOneAsync(x => x.Id == id, update)).MatchedCount == 1;
            }

            public Task<bool> CloseAsync(ObjectId id) => UpdateStatusAsync(id, TicketStatus.Closed);

            // Eén plek voor de statusregels, zodat UpdateAsync en UpdateStatusAsync nooit uit elkaar lopen.
            private static UpdateDefinition<Ticket> StatusFields(TicketStatus status, DateTime now)
            {
                var u = Builders<Ticket>.Update.Set(x => x.Status, status);
                return status switch
                {
                    TicketStatus.Open => u.Set(x => x.ResolvedAt, (DateTime?)null).Set(x => x.ClosedAt, (DateTime?)null),
                    TicketStatus.Resolved => u.Set(x => x.ResolvedAt, now).Set(x => x.ClosedAt, (DateTime?)null),
                    TicketStatus.Closed => u.Set(x => x.ClosedAt, now),   // resolvedAt blijft staan als het eerder opgelost was
                    _ => throw new ArgumentOutOfRangeException(nameof(status))
                };
            }

            private static void Validate(Ticket t)
            {
                if (string.IsNullOrWhiteSpace(t.Title)) throw new ArgumentException("Titel is verplicht.");
                if (string.IsNullOrWhiteSpace(t.Description)) throw new ArgumentException("Beschrijving is verplicht.");
                if (t.DateOfEvent == default) throw new ArgumentException("Datum van het incident is verplicht.");
                if (t.ReportedBy == ObjectId.Empty) throw new ArgumentException("Melder is verplicht.");
                if (!Enum.IsDefined(t.Priority)) throw new ArgumentException("Ongeldige prioriteit.");
            }
        }
}
