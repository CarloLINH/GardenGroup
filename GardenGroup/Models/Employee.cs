using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GardenGroup.Enums;

namespace GardenGroup.Models
{
    public class Employee
    {
        [BsonId] public ObjectId Id { get; set; }

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        
        public string Email { get; set; } = "";

        //hash
        public string PasswordHash { get; set; } = "";

        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; }
        //soft delete
        public bool IsActive { get; set; } = true;

        // Voor "forgot password" (latere sprint). Sla de HASH van het token op, niet het token zelf.
        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }

        [BsonIgnore] public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
