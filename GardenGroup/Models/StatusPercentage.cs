using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models;

[BsonIgnoreExtraElements]
public class StatusPercentage
{
    [BsonElement("status")] public string Status { get; set; } = "";
    [BsonElement("count")] public int Count { get; set; }
    [BsonElement("percentage")] public double Percentage { get; set; }
}