using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models;   

[BsonIgnoreExtraElements]
public class StatusPercentage
{
    
    public string Status { get; set; } = "";

    public int Count { get; set; }

    public double Percentage { get; set; }
}