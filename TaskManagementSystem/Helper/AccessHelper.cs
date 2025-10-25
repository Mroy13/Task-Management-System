using TaskManagementSystem.Models;
using TaskManagementSystem.Context;
using MongoDB.Driver;

namespace TaskManagementSystem.Helper
{
    public class AccessHelper
    {
        private readonly IMongoCollection<User> _userCollection;

        public AccessHelper(MongoDbContext context)
        {
            _userCollection = context.GetCollection<User>();
        }

        public async Task<int> checkRoleAccess(string id)
        {
            var user = await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            return (int) user.Role;
        }
    }
}
