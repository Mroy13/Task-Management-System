using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.ViewModels;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers;

/// <summary>Team listing, CRUD, and member assignment (role-gated where noted).</summary>
[ApiController]
[Route("Api/[Controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TeamController : ControllerBase
{
    private readonly TeamService _teamService;

    public TeamController(TeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>Returns a filtered, sorted, paginated list of teams.</summary>
    [HttpGet]
    public async Task<List<TeamResponse>> Get([FromQuery] TeamListQuery query)
    {
        return await _teamService.GetTeams(
            query.OrderBy,
            query.OrderType,
            query.SearchKey,
            query.SearchValue,
            query.Key,
            query.Page,
            query.PageSize);
    }

    /// <summary>Creates a new team or updates an existing one (admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("CreateTeam")]
    public async Task<IActionResult> CreateTeam(TeamData teamData)
    {
        if (teamData.Id is null || teamData.Id == "")
        {
            var teamSt = await _teamService.CreateTeam(teamData);
            if (teamSt.Id is not null)
                return CreatedAtAction(nameof(Get), new { id = teamData.Id }, teamData);
            return Unauthorized("Access Denied");
        }

        await _teamService.UpdateAsync(teamData);
        return Ok("updated");
    }

    /// <summary>Assigns members to a team (admin and team lead only).</summary>
    [Authorize(Roles = "Admin,TeamLead")]
    [HttpPost("AssignMember")]
    public async Task<IActionResult> AssignMember(TeamMemberAssignRequest request)
    {
        await _teamService.AssignMemberToTeam(request.TeamId, request.TeamMembers);
        return Ok("member assigned");
    }

    /// <summary>Returns the team led by the given user id.</summary>
    [HttpGet("GetTeam/{id:length(24)}")]
    public async Task<ActionResult<List<Team>>> GetTeamByTeamlead(string id)
    {
        var team = await _teamService.GetTeamsByTeamlead(id);
        return Ok(team);
    }

    /// <summary>Returns all teams led by the given user id.</summary>
    [HttpGet("GetTeamByLead/{id:length(24)}")]
    public async Task<ActionResult<List<Team>>> GetTeamsByTeamlead(string id)
    {
        var teams = await _teamService.GetTeamsByTeamlead(id);
        return Ok(teams);
    }

    /// <summary>Returns the members of the specified team.</summary>
    [HttpGet("GetMembers/{id:length(24)}")]
    public async Task<List<TeamMemberResponse>> GetTeamMembers(string id)
    {
        return await _teamService.GetMembersOfTeam(id);
    }

    /// <summary>Soft-deletes a team by id.</summary>
    [HttpPost("Delete/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var team = await _teamService.GetAsync(id);
        if (team is null)
            return NotFound();

        await _teamService.RemoveAsync(id);
        return Ok("Deleted");
    }
}
