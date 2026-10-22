using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace GardenGroup;

/// <summary>Collectienamen op één plek, zodat niemand een typefout in een string maakt.</summary>
public static class CollectionNames
{
    public const string Tickets = "tickets";
    public const string Employees = "employees";
}
public static class MongoConnection
{
    private static readonly object Lock = new();
    private static bool _conventionsRegistered;
    public static IMongoDatabase Create(string connectionString, string databaseName = "GardenGroup")
    {
        RegisterConventions();
        return new MongoClient(connectionString).GetDatabase(databaseName);
    }
    private static void RegisterConventions()
    {
        lock (Lock)
        {
            if (_conventionsRegistered) return;
            ConventionRegistry.Register("GardenGroupConventions", new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            }, _ => true);
            _conventionsRegistered = true;
        }
    }
}