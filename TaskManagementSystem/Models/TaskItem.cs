using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManagementSystem.Models;

/// <summary>Task document with assignments, comments, and activity history.</summary>
public class TaskItem
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

    [BsonRepresentation(BsonType.ObjectId)]
    public string AssignedBy { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string AssignedTo { get; set; }

    [BsonIgnoreIfNull]
    public List<AssignedTos> AssignedTos { get; set; }

    [BsonIgnoreIfNull]
    public List<Comments> Comments { get; set; }

    [BsonIgnoreIfNull]
    public List<Activity1> Activities { get; set; }

    public bool? IsDelete { get; set; } = false;
}

public enum Status
{
    [BsonRepresentation(BsonType.String)]
    Pending,
    [BsonRepresentation(BsonType.String)]
    InProgress,
    [BsonRepresentation(BsonType.String)]
    Completed,
}

public enum ActionType
{
    [BsonRepresentation(BsonType.String)]
    StatusChange,
    [BsonRepresentation(BsonType.String)]
    CrudOperation,
}

public class AssignedTos
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
}

public class Comments
{
    public Comments()
    {
        CreatedAt = DateTime.UtcNow;
        Id = ObjectId.GenerateNewId().ToString();
    }

    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Comment { get; set; }
    public AssignedTos CommentedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? IsDelete { get; set; } = false;
}

public class Activity1
{
    public Activity1()
    {
        CreatedAt = DateTime.UtcNow;
        Id = ObjectId.GenerateNewId().ToString();
    }

    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public ActionType? Actiontype { get; set; }

    [BsonIgnoreIfNull]
    public AssignedTos ExecutedBy { get; set; }

    [BsonIgnoreIfNull]
    public string OldValue { get; set; }

    [BsonIgnoreIfNull]
    public string NewValue { get; set; }

    [BsonIgnoreIfNull]
    public string Description { get; set; }

    public DateTime? CreatedAt { get; set; }
    public bool? IsDelete { get; set; } = false;
}
