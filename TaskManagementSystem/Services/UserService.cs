using MongoDB.Bson;
using MongoDB.Driver;
using TaskManagementSystem.Context;
using TaskManagementSystem.Helper;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.ViewModels;

namespace TaskManagementSystem.Services;

/// <summary>User CRUD, sign-in tokens, and related queries.</summary>
public class UserService
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<Team> _teamCollection;
    private readonly TeamService _teamService;
    private readonly Jwthelper _jwtHelper;

    public UserService(MongoDbContext context, Jwthelper jwthelper, TeamService teamService)
    {
        _userCollection = context.GetCollection<User>();
        _teamCollection = context.GetCollection<Team>();
        _teamService = teamService;
        _jwtHelper = jwthelper;
    }

    /// <summary>Returns a filtered, sorted, paginated list of users.</summary>
    public async Task<UserResponse> GetUsers(
        string orderBy,
        string orderType,
        string searchKey,
        string searchValue,
        int? page,
        int? pageSize,
        string key)
    {
        FilterDefinition<User> userFilter = Builders<User>.Filter.Empty;
        var skipCount = (page - 1) * pageSize;
        var userSortDefinition = Builders<User>.Sort.Combine();
        userSortDefinition = Builders<User>.Sort.Descending("Id");

        if (orderBy != null && orderBy != "" && orderType != null && orderBy != "")
        {
            if (orderType == "ascend")
                userSortDefinition = Builders<User>.Sort.Ascending(orderBy);
            else if (orderType == "descend")
                userSortDefinition = Builders<User>.Sort.Descending(orderBy);
        }

        userFilter &= Builders<User>.Filter.Eq("IsDelete", false);

        if (key != null && key != "")
        {
            userFilter &= Builders<User>.Filter.Regex("Name", new BsonRegularExpression(key, "i")) |
                          Builders<User>.Filter.Regex("UserName", new BsonRegularExpression(key, "i")) |
                          Builders<User>.Filter.Regex("Email", new BsonRegularExpression(key, "i")) |
                          Builders<User>.Filter.Regex("Role", new BsonRegularExpression(key, "i")) |
                          Builders<User>.Filter.Regex("Status", new BsonRegularExpression(key, "i"));
        }

        if (searchKey != null && searchKey != "" && searchValue != null && searchValue != " ")
        {
            userFilter &= Builders<User>.Filter.Regex(searchKey, new BsonRegularExpression(searchValue, "i"));
        }

        var totalcount = await _userCollection.CountDocumentsAsync(userFilter);
        var users = await _userCollection.Find(userFilter)
            .Sort(userSortDefinition)
            .Skip(skipCount)
            .Limit(pageSize)
            .ToListAsync();

        return new UserResponse { Users = users, TotalCount = totalcount };
    }

    /// <summary>Inserts a new user document.</summary>
    public async Task CreateUser(User userData) =>
        await _userCollection.InsertOneAsync(userData);

    /// <summary>Authenticates a user. Returns (response, error) where error is "inactive", "invalid", or null on success.</summary>
    public async Task<(SigninResponse response, string error)> SigninUser(SigninRequest userData)
    {
        var user = await _userCollection.Find(x => x.Email == userData.Email).FirstOrDefaultAsync();
        if (user == null || user.Password != userData.Password)
            return (null, "invalid");

        if (user.Status == UserStatus.InActive)
            return (null, "inactive");

        var token = _jwtHelper.GenerateJwtToken(user.Id, user.Email, user.Role);
        var refToken = _jwtHelper.GenerateJwtRefreshToken();
        var sigindata = new SigninResponse
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.Name,
            Token = token,
            RefToken = refToken,
            Email = user.Email,
            Role = (int)user.Role
        };
        return (sigindata, null);
    }

    /// <summary>Finds a user by email address.</summary>
    public async Task<User> GetUserByEmail(string email)
    {
        return await _userCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
    }

    public async Task AddRoleToUser(string id, int role)
    {
        var filter = Builders<User>.Filter.Eq("Id", id);
        var update = Builders<User>.Update.Set("Role", role);
        await _userCollection.UpdateOneAsync(filter, update);
    }

    public async Task ChangeuserStatus(string id, int status)
    {
        var filter = Builders<User>.Filter.Eq("Id", id);
        var update = Builders<User>.Update.Set("Status", status);
        await _userCollection.UpdateOneAsync(filter, update);
    }

    public async Task<List<User>> GetTeamLeaders()
    {
        return await _userCollection.Find(x => (int)x.Role == 2).ToListAsync();
    }

    public async Task<List<User>> GetMembersRealatedToUser(string userId)
    {
        var allMembers = new List<User>();
        var allTeams = await _teamService.GetTeams(null, null, "", "", null, null, null);
        foreach (var team in allTeams)
        {
            if (team.TeamMembers.Contains(new ObjectId(userId)))
            {
                var members = await _teamService.GetMembersOfTeam(team.Id);
                allMembers.AddRange(members[0].TeamMembers);
            }
        }

        return allMembers.DistinctBy(x => x.Id).ToList();
    }
}
