namespace TaskManagementSystem.Models;

/// <summary>Bound from configuration section TaskManagementDB.</summary>
public class TaskManagementDBSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string RoleCollectionName { get; set; }
}
