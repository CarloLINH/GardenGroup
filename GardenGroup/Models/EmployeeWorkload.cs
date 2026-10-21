using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models;

[BsonIgnoreExtraElements]
public class EmployeeWorkload
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public int TicketCount { get; set; }
}