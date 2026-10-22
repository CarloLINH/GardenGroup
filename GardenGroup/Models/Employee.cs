using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GardenGroup.Models;

public sealed class Employee
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("employeeId")]
    public int EmployeeId { get; set; }

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("password")]
    public string Password { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = string.Empty;

    [BsonElement("passwordResetTime")]
    public DateTime? PasswordResetTime { get; set; }

    [BsonElement("passwordResetLink")]
    public string? PasswordResetLink { get; set; }

    [BsonElement("isActive")]
    public bool? IsActive { get; set; }

    [BsonIgnore]
    public bool Active => IsActive != false;
}

public sealed class EmployeeFormViewModel
{
    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Employee ID")]
    public int EmployeeId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
