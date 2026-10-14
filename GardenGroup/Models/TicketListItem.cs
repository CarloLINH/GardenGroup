using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static GardenGroup.Enums.Enums;

namespace GardenGroup.Models
{
   //Een ticket in een lijstweergave, inclusief namen uit $lookup. Resultaat van aggregaties (deel 4).
    [BsonIgnoreExtraElements]
    public class TicketListItem
    {
        [BsonId] public ObjectId Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DateOfEvent { get; set; }
        public TicketPriority Priority { get; set; }

        [BsonRepresentation(BsonType.String)]
        public TicketStatus Status { get; set; }

        public ObjectId ReportedBy { get; set; }
        public ObjectId? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public string? ReporterName { get; set; }

        public string? AssigneeName { get; set; }
    }
}
