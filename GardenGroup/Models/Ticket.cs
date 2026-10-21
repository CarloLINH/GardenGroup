using GardenGroup.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models
{
    [BsonIgnoreExtraElements] 
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [BsonElement("ticketId")]
        public int TicketId { get; set; }

        [BsonElement("reportedBy")]
        public int ReportById { get; set; }

        [BsonElement("assignedTo")]
        public int? AssignedToId { get; set; }

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)] 
        public Statuses Status { get; set; } = Statuses.Open;

        [BsonElement("priority")]
        [BsonRepresentation(BsonType.String)] 
        public Priorities Priority { get; set; } = Priorities.Medium;

        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; } 

        [BsonElement("closedAt")]
        public DateTime? ClosedAt { get; set; } 
    }
}
