using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Models.ViewModels;

/// <summary>Projected task with populated user details, returned by task listing endpoints.</summary>
public class TaskResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string TaskName { get; set; }

    [BsonIgnoreIfNull]
    public string TaskDescription { get; set; }

    public DateTime Deadline { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Status Status { get; set; }

    /// <summary>Looked-up user documents for the assigner.</summary>
    public List<User> AssignedBy { get; set; }

    /// <summary>Looked-up user document for the assignee.</summary>
    public User AssignedToUser { get; set; }

    [BsonIgnoreIfNull]
    public List<AssignedTos> AssignedTos { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string AssignedTo { get; set; }

    /// <summary>Total matching documents (for pagination metadata).</summary>
    public long? TotalDocument { get; set; }

    [BsonIgnoreIfNull]
    public List<Comments> Comments { get; set; }
}

/// <summary>Request body for changing a task's status.</summary>
public class TaskChangeStatusRequest
{
    public string Id { get; set; }

    public int NewStatus { get; set; }

    public string UserId { get; set; }
}

/// <summary>Request body for deleting a specific comment from a task.</summary>
public class CommentDeleteRequest
{
    public string TaskId { get; set; }

    public string CommentId { get; set; }
}
