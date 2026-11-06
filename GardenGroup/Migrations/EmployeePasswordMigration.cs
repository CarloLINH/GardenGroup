using GardenGroup.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;

namespace GardenGroup.Migrations;

public static class EmployeePasswordMigration
{
    public static void Run(IMongoDatabase database, IPasswordHasher<Employee> passwordHasher)
    {
        var collection = database.GetCollection<BsonDocument>("employees");
        foreach (var document in collection.Find(new BsonDocument()).ToList())
        {
            if (!document.TryGetValue("password", out var legacyValue) || !legacyValue.IsString)
            {
                continue;
            }

            var employee = new Employee
            {
                EmployeeId = document.GetValue("employeeId", 0).AsInt32,
                Email = document.GetValue("email", string.Empty).AsString
            };
            var legacyPassword = legacyValue.AsString;
            var passwordHash = legacyPassword.StartsWith("AQAAAA", StringComparison.Ordinal)
                ? legacyPassword
                : passwordHasher.HashPassword(employee, legacyPassword);

            collection.UpdateOne(
                Builders<BsonDocument>.Filter.Eq("_id", document["_id"]),
                Builders<BsonDocument>.Update
                    .Set("passwordHash", passwordHash)
                    .Unset("password"));
        }
    }
}