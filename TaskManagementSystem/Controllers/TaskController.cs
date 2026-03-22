using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.ViewModels;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers;

/// <summary>Task CRUD, status changes, and comment management.</summary>
[ApiController]
[Route("Api/[Controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>Returns a single task by id.</summary>
    [HttpGet("{id:length(24)}")]
    public async Task<TaskItem> Get(string id)
    {
        return await _taskService.GetAsync(id);
    }

    /// <summary>Returns a filtered, sorted, paginated list of tasks.</summary>
    [HttpGet]
    public async Task<List<TaskResponse>> Get([FromQuery] TaskListQuery query)
    {
        var assignedTos = new AssignedTos { Id = query.AssignedTo, Name = query.AssignedToName };
        return await _taskService.GetTasks(
            query.Status,
            query.AssignedBy,
            query.AssignedTo,
            assignedTos,
            query.OrderBy,
            query.OrderType,
            query.Page,
            query.PageSize,
            query.FromDeadline,
            query.ToDeadline,
            query.SearchKey,
            query.SearchValue,
            query.Key);
    }

    /// <summary>Creates a new task or updates an existing one.</summary>
    [HttpPost("CreateUpd")]
    public async Task<IActionResult> CreateTask(TaskItem taskData)
    {
        if (taskData.Id is null || taskData.Id == "")
        {
            var taskSt = await _taskService.CreateTask(taskData);
            if (taskSt == 1)
                return CreatedAtAction(nameof(Get), new { id = taskData.Id }, taskData);
            return Unauthorized("Access Denied");
        }

        var taskdata = await _taskService.UpdateAsync(taskData);
        return Ok(taskdata);
    }

    /// <summary>Soft-deletes a task by id.</summary>
    [HttpPost("Delete/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _taskService.GetAsync(id);
        if (book is null)
            return NotFound();

        await _taskService.RemoveAsync(id);
        return NoContent();
    }

    /// <summary>Changes the status of a task and logs the activity.</summary>
    [HttpPost("Status")]
    public async Task<IActionResult> UpdateStatus([FromBody] TaskChangeStatusRequest item)
    {
        var task = await _taskService.GetAsync(item.Id);
        if (task is null)
            return NotFound();

        await _taskService.ChangeStatusAsync(item.Id, item.NewStatus, item.UserId);
        return NoContent();
    }

    /// <summary>Adds a comment to the specified task.</summary>
    [HttpPost("Comment/{id:length(24)}")]
    public async Task<IActionResult> CreateUpdComments(string id, Comments comment)
    {
        var task = await _taskService.GetAsync(id);
        if (task is null)
            return NotFound();

        await _taskService.AddCommentToTask(id, comment);
        return NoContent();
    }

    /// <summary>Updates an existing comment on the specified task.</summary>
    [HttpPost("UpdateComment/{id:length(24)}")]
    public async Task<IActionResult> UpdComments(string id, Comments comment)
    {
        var task = await _taskService.GetAsync(id);
        if (task is null)
            return NotFound();

        await _taskService.UpdateComment(id, comment.Id, comment);
        return NoContent();
    }

    /// <summary>Soft-deletes a specific comment from a task.</summary>
    [HttpPost("DeleteComment/Delete")]
    public async Task<IActionResult> DeleteComment(CommentDeleteRequest deleteParams)
    {
        var task = await _taskService.GetAsync(deleteParams.TaskId);
        if (task is null)
            return NotFound();

        await _taskService.DeleteComment(deleteParams.TaskId, deleteParams.CommentId);
        return NoContent();
    }
}
