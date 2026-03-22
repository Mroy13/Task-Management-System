using MongoDB.Driver;
using TaskManagementSystem.Context;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Helper;

/// <summary>
/// Reads a user's role from MongoDB by user id (used where business rules need current DB role).
/// </summary>
public class AccessHelper
{
    private readonly IMongoCollection<User> _userCollection;

    public AccessHelper(MongoDbContext context)
    {
        _userCollection = context.GetCollection<User>();
    }

    /// <summary>Returns the numeric role value for the user with the given id.</summary>
    public async Task<int> CheckRoleAccess(string id)
    {
        var user = await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (user is null)
            throw new InvalidOperationException("User not found.");
        return (int)user.Role;
    }
}
