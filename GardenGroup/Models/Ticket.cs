using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GardenGroup.Enums;

namespace GardenGroup.Models
{
    [BsonIgnoreExtraElements]
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ticketId")]
        [Display(Name = "Ticket number")]
        public int TicketId { get; set; }

        [BsonElement("reportedBy")]
        [Display(Name = "Reported by")]
        public int ReportById { get; set; }

        [BsonElement("assignedTo")]
        [Display(Name = "Assigned to")]
        public int? AssignedToId { get; set; }

        [BsonElement("title")]
        [Required(ErrorMessage = "Enter a title.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "The title must be between {2} and {1} characters.")]
        public required string Title { get; set; }

        [BsonElement("description")]
        [Required(ErrorMessage = "Describe the problem.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "The description must be between {2} and {1} characters.")]
        public string? Description { get; set; }

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        [Display(Name = "Type of incident")]
        public TicketTypes Type { get; set; } = TicketTypes.Other;

        [BsonElement("deadline")]
        [BsonDateTimeOptions(DateOnly = true)]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Deadline { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public Statuses Status { get; set; } = Statuses.Open;

        [BsonElement("priority")]
        public Priorities Priority { get; set; } = Priorities.Medium;

        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; }

        [BsonElement("closedAt")]
        public DateTime? ClosedAt { get; set; }

        [BsonElement("escalatedAt")]
        public DateTime? EscalatedAt { get; set; }
    }
}
