using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManagementSystem.Models
{
    public class Team
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TeamName { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string TeamLeader { get; set; } = null!;

        public TeamLeader? TeamLeaderDetails { get; set; }

        public List<ObjectId> TeamMembers { get; set; } = null!;

        //public List<string> TeamMembers { get; set; } = null!;

        public bool? IsDelete { get; set; } = false;
    }

    // public class TeamMembers
    //{
    //    public string ? Id { get; set; }
    //    public string ? Name { get; set; }


    //}

   public class TeamLeader
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }


}
