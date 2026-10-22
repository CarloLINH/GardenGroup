using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class EmployeeWorkload
{
    [BsonElement("employeeId")]
    public int EmployeeId { get; set; }

    [BsonElement("name")] public string Name { get; set; } = "";
    [BsonElement("ticketCount")] public int TicketCount { get; set; }
}