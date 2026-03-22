using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.ViewModels;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers;

/// <summary>User CRUD, sign-in (cookie + tokens), and admin role/status actions.</summary>
[ApiController]
[Route("Api/[Controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>Returns a filtered, sorted, paginated list of users.</summary>
    [HttpGet]
    public async Task<UserResponse> Get([FromQuery] UserListQuery query)
    {
        return await _userService.GetUsers(
            query.OrderBy,
            query.OrderType,
            query.SearchKey,
            query.SearchValue,
            query.Page,
            query.PageSize,
            query.Key);
    }

    /// <summary>Registers a new user (anonymous access).</summary>
    [AllowAnonymous]
    [HttpPost("Signup")]
    public async Task<IActionResult> CreateUser(User userData)
    {
        await _userService.CreateUser(userData);
        return CreatedAtAction(nameof(Get), new { id = userData.Id }, userData);
    }

    /// <summary>Authenticates a user, sets JWT cookies, and returns signin data.</summary>
    [AllowAnonymous]
    [HttpPost("Signin")]
    public async Task<ActionResult> Signin(SigninRequest signinData)
    {
        var (userData, signinError) = await _userService.SigninUser(signinData);

        if (signinError == "inactive")
            return Unauthorized("Access Denied:User In-Active!");

        if (userData == null)
            return BadRequest("invalid credential");

        Response.Cookies.Append("Jwt-token", userData.Token, new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(7),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        Response.Cookies.Append("ref-token", userData.RefToken, new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(7),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(userData);
    }

    /// <summary>Clears JWT cookies (placeholder for token refresh).</summary>
    [AllowAnonymous]
    [HttpGet("GetAccessToken")]
    public IActionResult GetAccessToken()
    {
        Response.Cookies.Delete("Jwt-token");
        Response.Cookies.Delete("ref-token");
        return Ok("logout");
    }

    /// <summary>Signs the user out by clearing JWT cookies.</summary>
    [AllowAnonymous]
    [HttpGet("Signout")]
    public IActionResult Signout()
    {
        Response.Cookies.Delete("Jwt-token");
        Response.Cookies.Delete("ref-token");
        return Ok("logout");
    }

    /// <summary>Assigns a role to a user (admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("AddRole")]
    public async Task<IActionResult> AddRoleToUser(AddRoleRequest role)
    {
        await _userService.AddRoleToUser(role.Id, role.Role);
        return Ok("Role Assigned");
    }

    /// <summary>Changes a user's active/inactive status.</summary>
    [HttpPost("ChangeStatus")]
    public async Task<IActionResult> ChangeUserStatus(UserStatusChangeRequest status)
    {
        await _userService.ChangeuserStatus(status.Id, status.Status);
        return Ok("Status Changed");
    }

    /// <summary>Returns all users with the TeamLead role.</summary>
    [HttpGet("TeamLeaders")]
    public async Task<ActionResult<List<User>>> GetTeamLeaders()
    {
        return await _userService.GetTeamLeaders();
    }

    /// <summary>Returns all team members related to the given user.</summary>
    [HttpGet("GetMembers/{id:length(24)}")]
    public async Task<List<User>> GetTeamMembers(string id)
    {
        return await _userService.GetMembersRealatedToUser(id);
    }
}
