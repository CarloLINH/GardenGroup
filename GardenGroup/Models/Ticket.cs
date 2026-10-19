using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models
{

    using GardenGroup.Enums;
    public class Ticket
    {
        [BsonId] public ObjectId Id { get; set; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        public DateTime DateOfEvent { get; set; }

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        [BsonRepresentation(BsonType.String)]
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        //verwijst naar employeeid
        public ObjectId ReportedBy { get; set; }
        //verwijst naar employee id
        public ObjectId? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

}

