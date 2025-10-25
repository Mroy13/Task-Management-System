using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TaskManagementSystem.Context;
using TaskManagementSystem.Helper;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services
{

    public class TaskResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TaskName { get; set; } = null!;

        public string? TaskDescription { get; set; } = null!;

        public DateTime Deadline { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Status Status { get; set; } = 0;

        public User[] AssignedBy { get; set; } = null!;

        public User AssignedToUser { get; set; } = null!;

        public List<AssignedTos>? AssignedTos { get; set; } =null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string AssignedTo { get; set; } = null!;

        public long? TotalDocument { get; set; }

        public List<Comments>? Comments { get; set; } = null!;




    }

    public class TaskService
    {
        private readonly IMongoCollection<Taskmo> _taskCollection;
        private readonly IMongoCollection<Team> _teamCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly AccessHelper _accessHelper;

        public TaskService(MongoDbContext context, AccessHelper accessHelper)
        {
            _taskCollection = context.GetCollection<Taskmo>();
            _teamCollection = context.GetCollection<Team>();
            _userCollection = context.GetCollection<User>();
            _accessHelper = accessHelper;
        }






        public async Task<Taskmo?> GetAsync(string id)
        {
            //var sortDefinition = Builders<Taskmo>.Sort.Ascending(doc => doc.Activities[0].Id);

            //var filter = Builders<Taskmo>.Filter.Eq("Id", id) &
            //             Builders<Taskmo>.Filter.ElemMatch(e => e.Activities, i => i.IsDelete == true);

            var task = await _taskCollection.Find(x=>x.Id==id)
                                            //.Sort(sortDefinition)
                                            
                                            .FirstOrDefaultAsync();

            var activities = task.Activities?.FindAll(x => x.IsDelete == false).OrderByDescending(x=>x.CreatedAt).ToList();
            var comments = task.Comments?.FindAll(x => x.IsDelete == false).OrderByDescending(x => x.CreatedAt).ToList();

            var newTask = new Taskmo() {
                Id = task.Id,
                TaskName = task.TaskName,
                TaskDescription =task.TaskDescription,
                Status = task.Status,
                Deadline = task.Deadline,
                AssignedBy = task.AssignedBy,
                AssignedTo = task.AssignedTo,
                AssignedTos = task.AssignedTos,
                Activities = activities,
                Comments = comments,
                IsDelete = false,
            };
            return newTask;
        }





        public async Task<List<TaskResponse>> GetTasks(int? status, string? assignedBy, string? assignedTo, AssignedTos? assignedTos, string? orderBy,
                                                       string? orderType,
                                                       long? Page, long? PageSize,DateTime?FromDeadline, DateTime? ToDeadline,
                                                       string? SearchKey, string? SearchValue,string? key
                                                       )
        {

            IAggregateFluent<TaskResponse> pipeline;
            FilterDefinition<Taskmo> taskFilter = Builders<Taskmo>.Filter.Empty;
            var taskSortDefination = Builders<Taskmo>.Sort.Combine();


           

           
            taskFilter &= Builders<Taskmo>.Filter.Eq("IsDelete", false);
            taskSortDefination = Builders<Taskmo>.Sort.Descending("Id");



            long? pageSize= await _taskCollection.CountDocumentsAsync(taskFilter);
            long? page = 1;

            //handle pagination
            if (Page!=null && PageSize != null)
            {
               
                pageSize = PageSize;
                page = Page;
                //Console.WriteLine(pageSize);
                //Console.WriteLine(page);

            }

            //set status filter
            if (status != null)
            {
               // Console.WriteLine($"st {status}");
                var st = (Status)status;
                taskFilter &= Builders<Taskmo>.Filter.Eq("Status", st);
            }


            //set search filter
            if(SearchKey != null && SearchKey!="" && SearchValue != null && SearchValue != " " && SearchKey!="AssignedBy")
            {
                if (SearchKey == "AssignedTos")
                {
                    
                    taskFilter &= Builders<Taskmo>.Filter
                                                  .ElemMatch(
                                                    x=>x.AssignedTos,
                                                    Builders<AssignedTos>.Filter.Regex("Name", new BsonRegularExpression(SearchValue, "i"))
                                                    //i=>i.Name==SearchValue
                                                  );

                }
                else
                    taskFilter &= Builders<Taskmo>.Filter.Regex(SearchKey, new BsonRegularExpression(SearchValue, "i"));
            }

            //generalize search:
            if(key!=null && key != "")
            {
                taskFilter &= Builders<Taskmo>.Filter.Regex("TaskName", new BsonRegularExpression(key, "i")) |
                              Builders<Taskmo>.Filter.Regex("TaskDescription", new BsonRegularExpression(key, "i")) |
                              Builders<Taskmo>.Filter.Regex("Status", new BsonRegularExpression(key, "i"))|
                              Builders<Taskmo>.Filter.Regex("AssignedBy", new BsonRegularExpression(key, "i"))|
                              Builders<Taskmo>.Filter
                                                  .ElemMatch(
                                                    x => x.AssignedTos,
                                                    Builders<AssignedTos>.Filter.Regex("Name", new BsonRegularExpression(key, "i"))
                                                  //i=>i.Name==SearchValue
                                                  );
            }
            //set sorting
            if(orderBy != null && orderBy!="" && orderType != null && orderBy!="")
            {
                if (orderType == "ascend")
                {
                   taskSortDefination = Builders<Taskmo>.Sort.Ascending(orderBy)
                                                             .Descending("Id");
                }
                else if (orderType == "descend")
                {
                   taskSortDefination= Builders<Taskmo>.Sort.Descending(orderBy)
                                                            .Descending("Id");
                }


            }

            //set deadline filter

            if (FromDeadline != null && ToDeadline != null)
            {
                taskFilter&= Builders<Taskmo>.Filter.Gte(x => x.Deadline, FromDeadline) &
                             Builders<Taskmo>.Filter.Lte(x => x.Deadline, ToDeadline);
            }



            //get tasks with filter and sorting:
            if (assignedTo == null && assignedBy == null && assignedTos?.Name == null)
            {
                var skipCount = (page - 1) * pageSize;
               // FilterDefinition<Taskmo> filter = Builders<Taskmo>.Filter(taskFilter);
                var count = await _taskCollection.CountDocumentsAsync(taskFilter);
               // Console.WriteLine("ab", assignedBy);
                pipeline = _taskCollection.Aggregate()
                                          .Match(taskFilter)
                                          //.SortByDescending(x => x.Id)
                                          .Sort(taskSortDefination)
                                          //.SortByDescending(x => x.Id)
                                          .Skip((long)skipCount)
                                          .Limit((long)pageSize)
                                          .Lookup<Taskmo, User, TaskResponse>(foreignCollection: _userCollection, localField: e => e.AssignedBy, 
                                                                              foreignField: f => f.Id, @as: o => o.AssignedBy)
                                          .Project(p => new TaskResponse
                                          {
                                              Id = p.Id,
                                              TaskName = p.TaskName,
                                              TaskDescription = p.TaskDescription,
                                              Status = p.Status,
                                              Deadline = p.Deadline,
                                              AssignedBy = p.AssignedBy,
                                              AssignedTo = p.AssignedTo,
                                              AssignedTos = p.AssignedTos,
                                              Comments = p.Comments,
                                              TotalDocument=count

                                          });
            }

            // Get user-spesific task
            //  else pipeline = GetUserSpesificTask(status, assignedBy, assignedTo, assignedTos);

            else
            {
                var skipCount = (page - 1) * pageSize;

                var count = await _taskCollection.CountDocumentsAsync(taskFilter);



                taskFilter &= Builders<Taskmo>.Filter.Where(x => x.AssignedTos.Contains(assignedTos) || x.AssignedBy == assignedBy);

                // Console.WriteLine("ab", assignedBy);
                pipeline = _taskCollection.Aggregate()
                                          .Match(taskFilter)
                                          //.SortByDescending(x => x.Id)
                                          .Sort(taskSortDefination)
                                          //.SortByDescending(x => x.Id)
                                          .Skip((long)skipCount)
                                          .Limit((long)pageSize)
                                          .Lookup<Taskmo, User, TaskResponse>(foreignCollection: _userCollection,localField: e => e.AssignedBy,
                                                                              foreignField: f => f.Id, @as: o => o.AssignedBy)
                                          .Project(p => new TaskResponse
                                          {
                                              Id = p.Id,
                                              TaskName = p.TaskName,
                                              TaskDescription = p.TaskDescription,
                                              Status = p.Status,
                                              Deadline = p.Deadline,
                                              AssignedBy = p.AssignedBy,
                                              AssignedTo = p.AssignedTo,
                                              AssignedTos = p.AssignedTos,
                                              Comments = p.Comments,
                                              TotalDocument = count

                                          });
            }




            //add assignedTo user Details

            var Tasks = await pipeline.ToListAsync();
            var newTasks = new List<TaskResponse>();

            foreach (var task in Tasks)
            {

                var AssignedToUser = await _userCollection.Find(x => x.Id == task.AssignedTo).FirstOrDefaultAsync();
                task.AssignedToUser = AssignedToUser;
                newTasks.Add(task);
            }


            //if (SearchKey == "AssignedBy")
            //{
            //    //FilterDefinition<Taskmo> innerFilter = Builders<Taskmo>.Filter.Empty;
            //    //innerFilter &= Builders<Taskmo>.Filter
            //    //                               .ElemMatch(
            //    //                                "AssignedBy",
            //    //                                Builders<Taskmo>.Filter.Regex("Name", new BsonRegularExpression(SearchValue, "i"))
            //    //                              );
                
            //    var AssignedBys = newTasks
            //                      .SelectMany(x=>x.AssignedBy)
            //                      .ToList()
            //                      .FindAll(x=>x.Name==SearchValue);
            //}


            //foreach (var task in newTasks)
            //{
            //    for ()
            //    {

            //    }
            //}
            return newTasks;

        }










        //public async Task CreateTask(Taskmo taskData) =>
        //    await _taskCollection.InsertOneAsync(taskData);



        //create task only with access:
        public async Task<int> CreateTask(Taskmo taskData)
        {

            try
            {
                int AssignedByRole = await _accessHelper.checkRoleAccess(taskData.AssignedBy);
                int AssignedToRole = await _accessHelper.checkRoleAccess(taskData.AssignedTo);
                var userData = await _userCollection.Find(x => x.Id == taskData.AssignedBy).FirstOrDefaultAsync();

                var executedBy= new AssignedTos() { Id = taskData.AssignedBy, Name=userData.Name};
                var activity = new Activity1() { Actiontype = (ActionType)1, ExecutedBy = executedBy, Description ="Task Created"};

                var activities = new List<Activity1>() { activity };

                //Console.WriteLine(taskData?.Activities);

                var newTaskData = new Taskmo()
                {

                    TaskName = taskData.TaskName,
                    TaskDescription = taskData.TaskDescription,
                    Status = taskData.Status,
                    Deadline = taskData.Deadline,
                    AssignedBy = taskData.AssignedBy,
                    AssignedTo = taskData.AssignedTo,
                    AssignedTos = taskData.AssignedTos,
                    Activities = activities,
                    Comments = new List<Comments> { }

                };


                //admin access
                if (AssignedByRole == 1)
                {
                    await _taskCollection.InsertOneAsync(newTaskData);
                    return 1;
                }


                //teamLead Access
                else if (AssignedByRole == 2)
                {
                    if (AssignedToRole == 1) return 0;
                    Team team = await _teamCollection.Find(x => x.TeamLeader == taskData.AssignedBy).FirstOrDefaultAsync();
                    //List<string> teamMembers = team.TeamMembers;
                    List<ObjectId> teamMembers = team.TeamMembers;
                    if (team.TeamLeader == taskData.AssignedTo)
                    {
                        await _taskCollection.InsertOneAsync(newTaskData);
                        return 1;
                    }
                    else if (teamMembers.Contains(new ObjectId(taskData.AssignedTo)))
                    {
                        await _taskCollection.InsertOneAsync(newTaskData);
                        return 1;
                    }
                    else
                        return 0;


                }


                //User Access
                else if (AssignedByRole == 0)
                {
                    //if (AssignedToRole == 1 || AssignedToRole == 2) return 0;
                    //if (taskData.AssignedBy != taskData.AssignedTo) return 0;
                    //else
                    //{
                    //    await _taskCollection.InsertOneAsync(taskData);
                    //    return 1;
                    //}

                    await _taskCollection.InsertOneAsync(newTaskData);
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw ex;
                
            }


        }









        //Update Task:

        public async Task<Taskmo> UpdateAsync(Taskmo updatedTask)
        {
            var task = await _taskCollection.Find(x => x.Id == updatedTask.Id).FirstOrDefaultAsync();
            if (task.IsDelete == true) return task;
            var userData = await _userCollection.Find(x => x.Id == updatedTask.AssignedBy).FirstOrDefaultAsync();

            var executedBy = new AssignedTos() { Id = updatedTask.AssignedBy, Name = userData.Name };
            var activity = new Activity1() { Actiontype = (ActionType)1, ExecutedBy = executedBy, Description = "Task Updated" };

            var activities = new List<Activity1>() {  };
            var comments = new List<Comments> { };
            if (task.Activities is not null)
            {
                activities = task.Activities;

            }

            if(task.Comments is not null)
            {
                comments = task.Comments;
            }

           

           // Console.WriteLine(updatedTask?.Activities);

            var newTaskData = new Taskmo()
            {
                Id=updatedTask.Id,
                TaskName = updatedTask.TaskName,
                TaskDescription = updatedTask.TaskDescription,
                Status = updatedTask.Status,
                Deadline = updatedTask.Deadline,
                AssignedBy = updatedTask.AssignedBy,
                AssignedTo = updatedTask.AssignedTo,
                AssignedTos = updatedTask.AssignedTos,
                Activities =activities,
                Comments = comments,
                IsDelete=false,

            };
            await _taskCollection.ReplaceOneAsync(x => x.Id == newTaskData.Id, newTaskData);
            var filter = Builders<Taskmo>.Filter.Eq("Id", newTaskData.Id);
            var update = Builders<Taskmo>.Update.AddToSet("Activities",activity);

            await _taskCollection.UpdateOneAsync(filter, update);
            var taskDetails = await _taskCollection.Find(x => x.Id == newTaskData.Id).FirstOrDefaultAsync();
            return taskDetails;
        }










        //Delete Task:



        public async Task RemoveAsync(string id)
        {
            var filter = Builders<Taskmo>.Filter.Eq("Id", id);
            var update = Builders<Taskmo>.Update.Set("IsDelete", true);
            await _taskCollection.UpdateOneAsync(filter, update);
        }






        //toggle status:


        public async Task ChangeStatusAsync(string id, int newStatus,string userId)
        {

           
            var userData = await _userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
            var task = await _taskCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            var oldvalue = "";
            var newvalue = "";
            //task.Status==0?oldvalue="Pending" :task.Status==1?oldvalue=

            if (task.Status == 0) oldvalue = "Pending";
            if (task.Status == (Status)1) oldvalue = "InProgress";
            if (task.Status == (Status)2) oldvalue = "Completed";

            if (newStatus == 0) newvalue = "Pending";
            if (newStatus == 1) newvalue = "InProgress";
            if (newStatus == 2) newvalue = "Completed";

            var executedBy = new AssignedTos() { Id =userData.Id, Name = userData.Name };

            var activity = new Activity1() { Actiontype = 0, ExecutedBy = executedBy,OldValue=oldvalue,NewValue=newvalue ,Description = "Status Changed" };

          //  var activities = new List<Activity1>() { };


            var filter = Builders<Taskmo>.Filter.Eq("Id", id);
            var update = Builders<Taskmo>.Update.Set("Status", newStatus)
                                                .AddToSet("Activities", activity);

            await _taskCollection.UpdateOneAsync(filter, update);

        }







        //add and update comments

        public async Task AddCommentToTask(string id,Comments comment)
        {
            //Console.WriteLine("id",id);
            var filter = Builders<Taskmo>.Filter.Eq("Id", id);
            var update = Builders<Taskmo>.Update.AddToSet("Comments", comment);

            await _taskCollection.UpdateOneAsync(filter, update);

        }


        public async Task UpdateComment(string taskId,string comId,Comments newComment)
        {
            //Console.WriteLine(taskId);
            var filter = Builders<Taskmo>.Filter.Eq("Id", taskId) &
                         Builders<Taskmo>.Filter.ElemMatch(e=>e.Comments,i=>i.Id==comId);

            var update = Builders<Taskmo>.Update.Set("Comments.$.Comment", newComment.Comment)
                                                .Set("Comments.$.CommentedBy", newComment.CommentedBy);


            await _taskCollection.UpdateOneAsync(filter, update);


        }

        public async Task DeleteComment(string taskId, string comId)
        {
            //Console.WriteLine(taskId);
            var filter = Builders<Taskmo>.Filter.Eq("Id", taskId) &
                         Builders<Taskmo>.Filter.ElemMatch(e => e.Comments, i => i.Id == comId);

            var update = Builders<Taskmo>.Update.Set("Comments.$.IsDelete", true);
                                                


            await _taskCollection.UpdateOneAsync(filter, update);


        }



    }
}
