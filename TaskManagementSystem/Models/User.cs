using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace TaskManagementSystem.Models
{

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = null!;
        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        [BsonRepresentation(BsonType.String)]

        public UserRole Role { get; set; } = 0;

        [BsonRepresentation(BsonType.String)]
        public UserStatus Status { get; set; } = 0;

        public bool? IsDelete { get; set; } = false;



    }

    public enum UserRole {
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

}
