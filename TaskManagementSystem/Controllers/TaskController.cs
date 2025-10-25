using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;
using static TaskManagementSystem.Services.TaskService;


namespace TaskManagementSystem.Controllers;

[ApiController]
[Route("Api/[Controller]")]


public class TaskController : ControllerBase

{
    private readonly TaskService _taskService;
    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }


    [HttpGet("{id:length(24)}")]
    public async Task<Taskmo> Get(string id)
    {
        //ObjectId Id = new ObjectId(AssignedTo);

      
        return await _taskService.GetAsync(id);


    }


    [HttpGet]
    public async Task<List<TaskResponse>> Get(int? status,string? AssignedBy,string? AssignedTo, string? AssignedToName,
                                              string? orderBy,string? orderType, long? Page, long? PageSize,
                                              DateTime? FromDeadline,DateTime? ToDeadline,string? SearchKey,string? SearchValue,
                                              string? Key
                                              )
    {
        //ObjectId Id = new ObjectId(AssignedTo);

        AssignedTos AssignedTos = new AssignedTos() { Id = AssignedTo, Name = AssignedToName };
        return await _taskService.GetTasks(status, AssignedBy, AssignedTo, AssignedTos, orderBy, orderType, Page,
                                      PageSize, FromDeadline, ToDeadline, SearchKey, SearchValue,Key);


    }

    [HttpPost("CreateUpd")]
    public async Task<IActionResult> CreateTask(Taskmo taskData)
    {

       // Console.WriteLine($"{taskData.Deadline}");
        if(taskData.Id is null || taskData.Id == "")
        {
            int taskSt = await _taskService.CreateTask(taskData);

            if (taskSt == 1)
                return CreatedAtAction(nameof(Get), new { id = taskData.Id }, taskData);
            else return Unauthorized("Access Denied");
        }

        else
        {
            var taskdata = await _taskService.UpdateAsync(taskData);
            return Ok(taskdata);
        }
        
    }




    [HttpPost("delete/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var book = await _taskService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _taskService.RemoveAsync(id);

        return NoContent();
    }




    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] changeStatus item)
    {

        var task = await _taskService.GetAsync(item.Id);

        if (task is null)
        {
            return NotFound();
        }

        //updatedTask.Id = task.Id;

        await _taskService.ChangeStatusAsync(item.Id, item.newStatus,item.UserId);

        return NoContent();
    }


    [HttpPost("comment/{id:length(24)}")]
    public async Task<IActionResult> CreateUpdComments(string Id, Comments comment)
    {
       

        var task = await _taskService.GetAsync(Id);

        if (task is null)
        {
            return NotFound();
        }

        //updatedTask.Id = task.Id;

       
        
            Console.WriteLine("cid", comment.Id);
           await _taskService.AddCommentToTask(Id, comment);
        

            

        //else if ((comment.Id != null)){
        //    await _taskService.UpdateComment(Id, comment.Id, comment);
        //}

        return NoContent();
    }



    [HttpPost("comment/upd/{id:length(24)}")]
    public async Task<IActionResult> UpdComments(string Id, Comments comment)
    {


        var task = await _taskService.GetAsync(Id);

        if (task is null)
        {
            return NotFound();
        }

        //updatedTask.Id = task.Id;



        Console.WriteLine("cid", comment.Id);
        await _taskService.UpdateComment(Id, comment.Id, comment);



        //else if ((comment.Id != null)){
        //    await _taskService.UpdateComment(Id, comment.Id, comment);
        //}

        return NoContent();
    }



    [HttpPost("comment/delete")]
    public async Task<IActionResult> DeleteComment(ComDelete deleteParams)
    {
        var task = await _taskService.GetAsync(deleteParams.TaskId);

        if (task is null)
        {
            return NotFound();
        }

        await _taskService.DeleteComment(deleteParams.TaskId,deleteParams.ComId);

        return NoContent();
    }



    public class changeStatus
    {
        public string Id { get; set; } = null!;
        public int newStatus { get; set; }
        public string UserId { get; set; } = null!;
    }


    public class Assignedtos
    {

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

   public class ComDelete {
        public string TaskId { get; set; } = null!;
        public string ComId { get; set; } = null!;

    }


}
