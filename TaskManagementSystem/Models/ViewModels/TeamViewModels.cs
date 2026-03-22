using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Models.ViewModels;

/// <summary>Projected team with populated leader details, returned by team listing endpoints.</summary>
public class TeamResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string TeamName { get; set; }

    /// <summary>Looked-up user documents for the team leader.</summary>
    public User[] TeamLeader { get; set; }

    public TeamLeader TeamLeaderDetails { get; set; }

    public List<ObjectId> TeamMembers { get; set; }

    /// <summary>Resolved user details for each team member.</summary>
    public List<User> TeamMembersDetails { get; set; }

    /// <summary>Total matching documents (for pagination metadata).</summary>
    public long? TotalDocument { get; set; }
}

/// <summary>Projected team with resolved member user details.</summary>
public class TeamMemberResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string TeamName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TeamLeader { get; set; }

    public List<User> TeamMembers { get; set; }
}

/// <summary>Request body for creating or updating a team.</summary>
public class TeamData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string TeamName { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TeamLeader { get; set; }

    public TeamLeader TeamLeaderDetails { get; set; }

    /// <summary>Member user ids as strings (converted to ObjectId on save).</summary>
    public List<string> TeamMembers { get; set; }

    public bool? IsDelete { get; set; } = false;
}

/// <summary>Request body for assigning members to a team.</summary>
public class TeamMemberAssignRequest
{
    public string TeamId { get; set; }

    public List<string> TeamMembers { get; set; }
}
