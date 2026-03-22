using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManagementSystem.Models;

/// <summary>Application user document stored in MongoDB.</summary>
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Name { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; }

    [BsonRepresentation(BsonType.String)]
    public UserStatus Status { get; set; }

    public bool? IsDelete { get; set; } = false;
}

public enum UserRole
{
    [BsonRepresentation(BsonType.String)]
    User,
    [BsonRepresentation(BsonType.String)]
    Admin,
    [BsonRepresentation(BsonType.String)]
    TeamLead
}

public enum UserStatus
{
    [BsonRepresentation(BsonType.String)]
    Active,
    [BsonRepresentation(BsonType.String)]
    InActive
}
