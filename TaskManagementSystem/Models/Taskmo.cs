using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManagementSystem.Models
{
    public class Taskmo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TaskName { get; set; } = null!;

        public string? TaskDescription { get; set; } = null!;

        //[BsonDateTimeOptions(DateOnly = true)]
        public DateTime Deadline { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Status Status { get; set; } = 0;

        [BsonRepresentation(BsonType.ObjectId)]

        public string AssignedBy { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]

        public string AssignedTo { get; set; } = null!;

        public List<AssignedTos>? AssignedTos { get; set; } = null!;

        public List<Comments>? Comments { get; set; } = null!;

        public List<Activity1>? Activities { get; set; }

        public bool? IsDelete { get; set; } = false;



    }

    public enum Status {
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
        //[BsonRepresentation(BsonType.String)]
        //Completed,
    }

    public class AssignedTos {

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class Comments
    {
        public Comments()
        {
            CreatedAt = DateTime.UtcNow;
            Id = ObjectId.GenerateNewId().ToString();
        }

        
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Comment { get; set; } = null!;
        public AssignedTos CommentedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public bool? IsDelete { get; set; } = false;


    }



    public class Activity1 {
        public Activity1()
        {
            CreatedAt = DateTime.UtcNow;
            Id = ObjectId.GenerateNewId().ToString();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public ActionType? Actiontype { get; set; } = 0;

        public AssignedTos? ExecutedBy { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }

        public bool? IsDelete { get; set; } = false;




    }



}
