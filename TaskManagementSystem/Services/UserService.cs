using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Buffers;
using System.Data;
using TaskManagementSystem.Context;
using TaskManagementSystem.Helper;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Services
{

    public class SigninData
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class SigninResponse
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Token { get; set; } = null!;

        public string RefToken { get; set; } = null!;

        public int Role { get; set; }
    }

    public class UserResponse
    {
        public List<User> Users { get; set; } = null!;

        public long? TotalCount { get; set; }



    }

    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Team> _teamCollection;
        private readonly TeamService _teamService;
        private readonly Jwthelper _jwtHelper;

        public UserService(MongoDbContext context,Jwthelper jwthelper,TeamService teamService)
        {
            _userCollection = context.GetCollection<User>();
            _teamCollection = context.GetCollection<Team>();
            _teamService = teamService;
            _jwtHelper = jwthelper;

        }




        public async Task<UserResponse> GetUsers(string?orderBy,string? orderType,string? SearchKey,string? SearchValue,int? Page,int? PageSize,string? key )
        {
            FilterDefinition<User> userFilter = Builders<User>.Filter.Empty;
            long count = await _userCollection.CountDocumentsAsync(userFilter);
            var skipCount = (Page - 1) * PageSize;

            var userSortDefination = Builders<User>.Sort.Combine();

            //set sorting
            userSortDefination = Builders<User>.Sort.Descending("Id");

            if (orderBy != null && orderBy != "" && orderType != null && orderBy != "")
            {
                Console.WriteLine($"{orderBy},{orderType}");
                if (orderType == "ascend")
                {
                    userSortDefination = Builders<User>.Sort.Ascending(orderBy);
                }
                else if (orderType == "descend")
                {
                    userSortDefination = Builders<User>.Sort.Descending(orderBy);
                }


            }

            userFilter &= Builders<User>.Filter.Eq("IsDelete", false);

            if(key!=null && key != "")
            {
                userFilter &= Builders<User>.Filter.Regex("Name", new BsonRegularExpression(key, "i")) |
                              Builders<User>.Filter.Regex("UserName", new BsonRegularExpression(key, "i")) |
                              Builders<User>.Filter.Regex("Email", new BsonRegularExpression(key, "i")) |
                              Builders<User>.Filter.Regex("Role", new BsonRegularExpression(key, "i"))|
                              Builders<User>.Filter.Regex("Status", new BsonRegularExpression(key, "i"));
            }

            if (SearchKey != null && SearchKey != "" && SearchValue != null && SearchValue != " ")
            {
                userFilter &= Builders<User>.Filter.Regex(SearchKey, new BsonRegularExpression(SearchValue, "i"));
            }

            var totalcount = await _userCollection.CountDocumentsAsync(userFilter);
            var users= await _userCollection.Find(userFilter)
                                            //.SortByDescending(x => x.Id)
                                            .Sort(userSortDefination)
                                            .Skip(skipCount)
                                            .Limit(PageSize)
                                            .ToListAsync();


            var userResponse = new UserResponse() { Users = users, TotalCount =totalcount };
            return userResponse;

        }



        public async Task CreateUser(User userData) =>
            await _userCollection.InsertOneAsync(userData);





        public async Task<SigninResponse> signinUser(SigninData userData)

        {
            var user = await _userCollection.Find((x) => x.Email == userData.Email).FirstOrDefaultAsync();
            if (user == null || user?.Password != userData.Password) return null;

            else
            {
                var token = _jwtHelper.GenerateJwtToken(user.Email);
                var refToken = _jwtHelper.GenerateJwtRefreshToken();
                SigninResponse sigindata = new SigninResponse(){ Id=user.Id,Name=user.Name,UserName=user.Name,Token=token,RefToken=refToken,Email=user.Email,Role=(int)user.Role};
                return sigindata;
            }

        }



        public async Task<User>GetUserByEmail(string email)
        {
            User user = await _userCollection.Find((x) => x.Email == email).FirstOrDefaultAsync();
            return user;
        }




        //assign role to user(Admin feature)
        public async Task AddRoleToUser(string id, int role)
        {
            var filter = Builders<User>.Filter.Eq("Id", id);
            var update = Builders<User>.Update.Set("Role", role);
            await _userCollection.UpdateOneAsync(filter, update);
        }


        public async Task ChangeuserStatus(string id, int status)
        {
            Console.WriteLine($"{id}, {status}");
            var filter = Builders<User>.Filter.Eq("Id", id);
            var update = Builders<User>.Update.Set("Status", status);
            await _userCollection.UpdateOneAsync(filter, update);
        }



        public async Task<List<User>> GetTeamLeaders()
        {
           
           return await _userCollection.Find(x =>(int)x.Role == 2).ToListAsync();
        }





        //Get all same team members with User:
        public async Task<List<User>> GetMembersRealatedToUser(string userId)
        {
            //List<TeamMemberResponse> allMembers=new List<TeamMemberResponse>() { };
            List<User> allMembers = new List<User>() { };

            var allTeams = await _teamService.GetTeams(null,null,"","",null,null,null);
            foreach (var team in allTeams)
            {
                if (team.TeamMembers.Contains(new ObjectId(userId)))
                {
                    List<TeamMemberResponse> memebers = await _teamService.GetMembersOfTeam(team.Id);
                    allMembers.AddRange(memebers[0].TeamMembers);
                }
            }

            return allMembers.DistinctBy(x => x.Id).ToList();
        }


    }
}
