using GardenGroup.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models;

/// <summary>
/// Aantal en percentage tickets in één dashboardcategorie.
/// Resultaat van de aggregatiepipeline in <c>TicketQueries.GetStatusPercentagesAsync</c>.
/// </summary>
[BsonIgnoreExtraElements]
public class StatusPercentage
{
    /// <summary>Open, Resolved of ClosedWithoutResolve (zie <see cref="TicketCategory"/>).</summary>
    [BsonElement("category")]
    [BsonRepresentation(BsonType.String)]
    public TicketCategory Category { get; set; }

    /// <summary>Aantal tickets in deze categorie.</summary>
    [BsonElement("count")]
    public int Count { get; set; }

    /// <summary>Percentage 0 tot 100, afgerond op één decimaal.</summary>
    [BsonElement("percentage")]
    public double Percentage { get; set; }
}
