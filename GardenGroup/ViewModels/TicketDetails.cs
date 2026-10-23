using GardenGroup.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.ViewModels
{
    public class TicketDetails : Ticket
    {
        [BsonIgnore]
        public Employee? Reporter { get; init; }

        [BsonIgnore]
        public Employee? Assignee { get; init; }
    }
}
