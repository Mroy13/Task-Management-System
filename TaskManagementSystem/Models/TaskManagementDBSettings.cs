namespace TaskManagementSystem.Models
{
    public class TaskManagementDBSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string RoleCollectionName { get; set; } = null!;
    }
}
