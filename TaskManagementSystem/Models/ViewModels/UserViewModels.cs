using TaskManagementSystem.Models;

namespace TaskManagementSystem.Models.ViewModels;

/// <summary>Request body for user sign-in.</summary>
public class SigninRequest
{
    public string Email { get; set; }

    public string Password { get; set; }
}

/// <summary>Response returned after a successful sign-in, includes JWT tokens.</summary>
public class SigninResponse
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    /// <summary>Short-lived JWT access token.</summary>
    public string Token { get; set; }

    /// <summary>Longer-lived JWT refresh token.</summary>
    public string RefToken { get; set; }

    public int Role { get; set; }
}

/// <summary>Paginated list of users.</summary>
public class UserResponse
{
    public List<User> Users { get; set; }

    /// <summary>Total documents matching the filter (for pagination).</summary>
    public long? TotalCount { get; set; }
}

/// <summary>Request body for assigning a role to a user (admin only).</summary>
public class AddRoleRequest
{
    public string Id { get; set; }

    public int Role { get; set; }
}

/// <summary>Request body for changing a user's active/inactive status.</summary>
public class UserStatusChangeRequest
{
    public string Id { get; set; }

    public int Status { get; set; }
}
