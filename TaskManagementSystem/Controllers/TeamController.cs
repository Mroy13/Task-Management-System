using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers;

[ApiController]
[Route("Api/[Controller]")]

public class TeamController:ControllerBase
{
    private readonly TeamService _teamService;

    public TeamController(TeamService teamService)
    {
        _teamService = teamService;
    }


    [HttpGet]
    public async Task<List<TeamResponse>> Get(string? orderBy, string? orderType, string? SearchKey, string? SearchValue,string? Key,long? Page,long? PageSize)
    {
       
       
       return await _teamService.GetTeams(orderBy, orderType,SearchKey,SearchValue,Key,Page,PageSize);

    }
     




    //admin feature

    [HttpPost("createTeam")]
    public async Task<IActionResult> CreateTeam(TeamData teamData)
    {
        //var teamdata= await _teamService.CreateTeam(teamData);
        //return teamdata;
        //Console.WriteLine("inside");
        if (teamData.Id is null || teamData.Id == "")
        {
            Team teamSt = await _teamService.CreateTeam(teamData);
            if (teamSt.Id is not null)
                return CreatedAtAction(nameof(Get), new { id = teamData.Id }, teamData);
            else return Unauthorized("Access Denied");
        }

        else
        {
            var teamdata = await _teamService.UpdateAsync(teamData);
            return Ok("updated");
        }
    }






    //TeamLead and Admin feature

    [HttpPost("AssignMember")]
    public async Task<IActionResult> AssignMember(TeamMember teammember)
    {
        await _teamService.AssignMemberToTeam(teammember.teamId, teammember.teamMembers);
        return Ok("member assigned");
    }



    [HttpGet("Getteam/{id:length(24)}")]
    public async Task<ActionResult<List<Team>>> GetTeamByTeamlead(string id)
    {
        var team = await _teamService.GetTeamsByTeamlead(id);
        return Ok(team);
    }


    [HttpGet("GetteamByLead/{id:length(24)}")]
    public async Task<ActionResult<List<Team>>> GetTeamsByTeamlead(string id)
    {
        var teams = await _teamService.GetTeamsByTeamlead(id);
        return Ok(teams);
    }




    [HttpGet("GetMembers/{id:length(24)}")]
    public async Task<List<TeamMemberResponse>> GetTeamMembers(string id)
    {
        var team = await _teamService.GetMembersOfTeam(id);
        return team;
    }





    [HttpPost("delete/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var team = await _teamService.GetAsync(id);

        if (team is null)
        {
            return NotFound();
        }

        await _teamService.RemoveAsync(id);

        return Ok("Deleted");
    }







    public class TeamMember {
       public string teamId { get; set; } = null!;
       public List<string> teamMembers { get; set; } = null!;
    }



    

}
