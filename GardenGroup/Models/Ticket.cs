using GardenGroup.Enums;

namespace GardenGroup.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int ReportById { get; set; }
        public int? AssignedToId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public Statuses Status { get; set; } = Statuses.Open;

        public Priorities Priority { get; set; } = Priorities.Medium;

        public DateTime ResolvedAt { get; set; }

        public DateTime ClosedAt { get; set; }
    }
}
