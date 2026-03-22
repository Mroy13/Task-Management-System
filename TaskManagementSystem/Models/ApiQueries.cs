namespace TaskManagementSystem.Models;

/// <summary>Optional query parameters for GET Api/User (model-bound from query string).</summary>
public class UserListQuery
{
    public string OrderBy { get; set; }
    public string OrderType { get; set; }
    public string SearchKey { get; set; }
    public string SearchValue { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string Key { get; set; }
}

/// <summary>Optional query parameters for GET Api/Task.</summary>
public class TaskListQuery
{
    public int? Status { get; set; }
    public string AssignedBy { get; set; }
    public string AssignedTo { get; set; }
    public string AssignedToName { get; set; }
    public string OrderBy { get; set; }
    public string OrderType { get; set; }
    public long? Page { get; set; }
    public long? PageSize { get; set; }
    public DateTime? FromDeadline { get; set; }
    public DateTime? ToDeadline { get; set; }
    public string SearchKey { get; set; }
    public string SearchValue { get; set; }
    public string Key { get; set; }
}

/// <summary>Optional query parameters for GET Api/Team.</summary>
public class TeamListQuery
{
    public string OrderBy { get; set; }
    public string OrderType { get; set; }
    public string SearchKey { get; set; }
    public string SearchValue { get; set; }
    public string Key { get; set; }
    public long? Page { get; set; }
    public long? PageSize { get; set; }
}
