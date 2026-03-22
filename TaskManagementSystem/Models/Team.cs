using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManagementSystem.Models;

/// <summary>Team document with leader and member object ids.</summary>
public class Team
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string TeamName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TeamLeader { get; set; }

    [BsonIgnoreIfNull]
    public TeamLeader TeamLeaderDetails { get; set; }

    public List<ObjectId> TeamMembers { get; set; }

    public bool? IsDelete { get; set; } = false;
}

/// <summary>Embedded leader summary.</summary>
public class TeamLeader
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
}
