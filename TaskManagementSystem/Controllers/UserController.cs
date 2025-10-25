using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers;

[ApiController]
[Route("Api/[Controller]")]

public class UserController:ControllerBase
{

    private readonly UserService _userService;


    public UserController(UserService userService)
    {
        _userService = userService;
    }



    [HttpGet]
    public async Task<UserResponse> Get(string?orderBy,string?orderType, string? SearchKey, string? SearchValue,
                                        int? Page,int? PageSize,string? Key)
    {
         
     
     return await _userService.GetUsers(orderBy,orderType,SearchKey,SearchValue,Page,PageSize,Key);

    }




    [HttpPost("signup")]
    public async Task<IActionResult> CreateUser(User userData)
    {
        await _userService.CreateUser(userData);
        return CreatedAtAction(nameof(Get), new { id = userData.Id }, userData);
    }





    [HttpPost("signin")]

    public async Task<ActionResult> signin(SigninData siginindata)
    {
        var userData = await _userService.signinUser(siginindata);

        if (userData == null)
        {
            return BadRequest("invalid credential");

        }
        else
        {
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



    }




    [HttpGet("getAccessToken")]
    public IActionResult GetAccessTokenAsync()
    {

        Response.Cookies.Delete("Jwt-token");
        Response.Cookies.Delete("ref-token");
        return Ok("logout");


    }



    [HttpGet("signout")]
    public IActionResult UserSignoutAsync()
    {

        Response.Cookies.Delete("Jwt-token");
        Response.Cookies.Delete("ref-token");
        return Ok("logout");


    }








    //admin feature add role to user
    [HttpPost("AddRole")]

    public async Task<IActionResult> AddRoleToUser(AddRole role)
    {
        await _userService.AddRoleToUser(role.id,role.Role);
        //return CreatedAtAction(nameof(Get), new { id = teamData.Id }, teamData);

        return Ok("Role Assigned");

    }


    [HttpPost("ChangeStatus")]

    public async Task<IActionResult> ChangeUserStatus(ChangeStatus status)
    {
        await _userService.ChangeuserStatus(status.id, status.Status);
        //return CreatedAtAction(nameof(Get), new { id = teamData.Id }, teamData);

        return Ok("Status Changed");

    }




    [HttpGet("TeamLeaders")]

    public async Task<ActionResult<List<User>>> GetTeamLeaders()
    {
       return await _userService.GetTeamLeaders();
        //return CreatedAtAction(nameof(Get), new { id = teamData.Id }, teamData);



    }



    [HttpGet("GetMembers/{id:length(24)}")]
    public async Task<List<User>> GetTeamMembers(string id)
    {
        var team = await _userService.GetMembersRealatedToUser(id);
        return team;
    }


    public class AddRole {
        public string id { get; set; } = null!;
        public int Role { get; set; }
    }


    public class ChangeStatus
    {
        public string id { get; set; } = null!;
        public int Status { get; set; }
    }







}
