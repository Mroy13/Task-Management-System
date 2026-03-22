using MongoDB.Bson;
using MongoDB.Driver;
using TaskManagementSystem.Context;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.ViewModels;

namespace TaskManagementSystem.Services;

    /// <summary>Team CRUD, member assignment, and aggregation queries.</summary>
    public class TeamService
    {
        private readonly IMongoCollection<Team> _teamCollection;
        private readonly IMongoCollection<User> _userCollection;

        public TeamService(MongoDbContext context)
        {
            _teamCollection = context.GetCollection<Team>();
            _userCollection = context.GetCollection<User>();
        }




        /// <summary>Retrieves a single team by id.</summary>
        public async Task<Team> GetAsync(string id)
        {
            var team = await _teamCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return team;
        }






        /// <summary>Returns a filtered, sorted, paginated list of teams with user lookups.</summary>
        public async Task<List<TeamResponse>> GetTeams(string orderBy, string orderType, string SearchKey, string SearchValue, string key, long? page, long? pageSize)
        {
            IAggregateFluent<TeamResponse> pipeline;
            var sortDefinition = Builders<TeamResponse>.Sort.Combine();
            FilterDefinition<Team> teamFilter = Builders<Team>.Filter.Empty;
            sortDefinition = Builders<TeamResponse>.Sort.Descending("Id");
            long? skipCount=0;
           

            if (orderBy!=null && orderType != null)
            {
               
                if (orderType == "ascend")
                {
                    sortDefinition = Builders<TeamResponse>.Sort.Ascending(orderBy);
                }
                else if (orderType == "descend")
                {
                    sortDefinition = Builders<TeamResponse>.Sort.Descending(orderBy);
                }

            }
            //else
            //{
            //     pipeline = _teamCollection.Aggregate()
            //                           .Match(x => x.IsDelete == false)
            //                           .SortByDescending(x => x.Id)
            //                           .Lookup<Team, User, TeamResponse>(foreignCollection: _userCollection, localField: e => e.TeamLeader, foreignField: f => f.Id, @as: o => o.TeamLeader)
            //                           //.Unwind<TeamResponse,Team>(e=>e.TeamLeader)
            //                           //.Lookup<Team, User, TeamResponse>(foreignCollection: _userCollection, localField: e => e.TeamMembers, foreignField: f => f.Id, @as: o => o.TeamMembers)
            //                           .Project(p => new TeamResponse
            //                           {
            //                               Id = p.Id,
            //                               TeamName = p.TeamName,
            //                               TeamLeader = p.TeamLeader,
            //                               TeamLeaderDetails = p.TeamLeaderDetails,
            //                               TeamMembers = p.TeamMembers

            //                           });
            //}


            teamFilter &= Builders<Team>.Filter.Eq("IsDelete", false);
            if(key!=null && key != "")
            {
                teamFilter &= Builders<Team>.Filter.Regex("TeamName", new BsonRegularExpression(key, "i")) |
                              Builders<Team>.Filter.Regex(x => x.TeamLeaderDetails.Name, new BsonRegularExpression(key, "i"));
            }
            if (SearchKey != null && SearchKey != "" && SearchValue != null && SearchValue != " ")
            {
                if (SearchKey =="TeamLeader")
                {
                    SearchKey = "TeamLeaderDetails";
                    teamFilter &= Builders<Team>.Filter.Regex(x => x.TeamLeaderDetails.Name, new BsonRegularExpression(SearchValue, "i"));
                }
                else
                    teamFilter &= Builders<Team>.Filter.Regex(SearchKey, new BsonRegularExpression(SearchValue, "i"));
            }



            if (page != null) skipCount = (page - 1) * pageSize;
            pageSize = await _teamCollection.CountDocumentsAsync(teamFilter);

            pipeline = _teamCollection.Aggregate()
                                       .Match(teamFilter)
                                       .Lookup<Team, User, TeamResponse>(foreignCollection: _userCollection, localField: e => e.TeamLeader, foreignField: f => f.Id, @as: o => o.TeamLeader)
                                       .Sort(sortDefinition)
                                       .Skip((long)skipCount)
                                       .Limit((long)pageSize)
                                       //.Unwind<TeamResponse,Team>(e=>e.TeamLeader)
                                       //.Lookup<Team, User, TeamResponse>(foreignCollection: _userCollection, localField: e => e.TeamMembers, foreignField: f => f.Id, @as: o => o.TeamMembers)
                                       .Project(p => new TeamResponse
                                       {
                                           Id = p.Id,
                                           TeamName = p.TeamName,
                                           TeamLeader = p.TeamLeader,
                                           TeamLeaderDetails = p.TeamLeaderDetails,
                                           TeamMembers = p.TeamMembers,
                                           TotalDocument=pageSize


                                       });

            var teams = await pipeline.ToListAsync();
            var newTeams = new List<TeamResponse>();


            foreach (var team in teams)
            {
                var teamMembers = await GetMembersOfTeam(team.Id);

                team.TeamMembersDetails = teamMembers[0].TeamMembers;

                newTeams.Add(team);



            }




            return newTeams;

            //return await pipeline.ToListAsync();

            //await _teamCollection.Find(x => x.IsDelete == false).SortByDescending(x => x.Id).ToListAsync();
        }








        public async Task<Team> CreateTeam(TeamData teamData)
        {
            Team newteamData = new Team() { };
            newteamData.TeamName = teamData.TeamName;
            newteamData.TeamLeader = teamData.TeamLeader;
            newteamData.TeamLeaderDetails = teamData.TeamLeaderDetails;
            newteamData.TeamMembers = [];
            foreach (var memberId in teamData.TeamMembers)
            {
                newteamData.TeamMembers.Add(new ObjectId(memberId));
            }
            await _teamCollection.InsertOneAsync(newteamData);
            var teamdata = await GetTeamByTeamlead(teamData.TeamLeader);
            return teamdata;
        }




        //Update Team:

        public async Task<Team> UpdateAsync(TeamData updatedTeam)
        {
            var team = await _teamCollection.Find(x => x.Id == updatedTeam.Id).FirstOrDefaultAsync();
            if (team.IsDelete == true) return team;
            Team newteamData = new Team() { };
            newteamData.Id = updatedTeam.Id;
            newteamData.TeamName = updatedTeam.TeamName;
            newteamData.TeamLeader = updatedTeam.TeamLeader;
            newteamData.TeamLeaderDetails = updatedTeam.TeamLeaderDetails;
            newteamData.TeamMembers = [];
            foreach (var memberId in updatedTeam.TeamMembers)
            {
                newteamData.TeamMembers.Add(new ObjectId(memberId));
            }
            await _teamCollection.ReplaceOneAsync(x => x.Id == newteamData.Id, newteamData);
            var teamDetails = await _teamCollection.Find(x => x.Id == updatedTeam.Id).FirstOrDefaultAsync();
            return teamDetails;
        }







        //TeamLead and Admin feature(Assign member to the team)

        public async Task AssignMemberToTeam(string id, List<string> members)
        {
            Team team = await _teamCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            //List<string> existingMembers = team.TeamMembers;
            //List<string> newMembers = [];

            List<ObjectId> existingMembers = team.TeamMembers;
            List<ObjectId> newMembers = [];


            foreach (var member in members)
            {
                ObjectId memberObject = new ObjectId(member);

                if (!existingMembers.Contains(memberObject))
                {
                    newMembers.Add(memberObject);
                }
            }
            existingMembers.AddRange(newMembers);
            var filter = Builders<Team>.Filter.Eq("Id", id);
            var update = Builders<Team>.Update.Set("TeamMembers", existingMembers);
            await _teamCollection.UpdateOneAsync(filter, update);

        }





        //TeamLead Feature(Get-All teams by teamlead):



        public async Task<Team> GetTeamByTeamlead(string id)
        {
            return await _teamCollection.Find(x => x.TeamLeader == id).FirstOrDefaultAsync();

        }

        public async Task<List<TeamResponse>> GetTeamsByTeamlead(string id)
        {
            IAggregateFluent<TeamResponse> pipeline;

            pipeline = _teamCollection.Aggregate()
                                       .Match(x => x.IsDelete == false)
                                       .Match(x => x.TeamLeaderDetails.Id == id)
                                       .SortByDescending(x => x.Id)
                                       .Lookup<Team, User, TeamResponse>(foreignCollection: _userCollection, localField: e => e.TeamLeader, foreignField: f => f.Id, @as: o => o.TeamLeader)
                                       //.Unwind<TeamResponse,Team>(e=>e.TeamLeader)
                                       //.Lookup<Team, User, TeamResponse>(foreignCollection: _userCollection, localField: e => e.TeamMembers, foreignField: f => f.Id, @as: o => o.TeamMembers)
                                       .Project(p => new TeamResponse
                                       {
                                           Id = p.Id,
                                           TeamName = p.TeamName,
                                           TeamLeader = p.TeamLeader,
                                           TeamLeaderDetails = p.TeamLeaderDetails,
                                           TeamMembers = p.TeamMembers


                                       });

            var teams = await pipeline.ToListAsync();
            var newTeams = new List<TeamResponse>();


            foreach (var team in teams)
            {
                var teamMembers = await GetMembersOfTeam(team.Id);

                team.TeamMembersDetails = teamMembers[0].TeamMembers;

                newTeams.Add(team);



            }




            return newTeams;
            //return await _teamCollection.Find(x => x.TeamLeaderDetails.Id == id).ToListAsync();

        }




        //  TeamLead Feature(Get-Members of Team):

        public async Task<List<TeamMemberResponse>> GetMembersOfTeam(string id)
        {
            var pipeline = _teamCollection.Aggregate()
                                           .Match(x => x.Id == id)
                                           .SortByDescending(x => x.Id)
                                           .Lookup<Team, User, TeamMemberResponse>(foreignCollection: _userCollection, localField: e => e.TeamMembers, foreignField: f => f.Id, @as: o => o.TeamMembers)
                                           .Project(p => new TeamMemberResponse
                                           {
                                               Id = p.Id,
                                               TeamName = p.TeamName,
                                               TeamLeader = p.TeamLeader,
                                               TeamMembers = p.TeamMembers,

                                           });


            return await pipeline.ToListAsync();

        }




        //Delete Team(admin feature):

        public async Task RemoveAsync(string id)
        {
            var filter = Builders<Team>.Filter.Eq("Id", id);
            var update = Builders<Team>.Update.Set("IsDelete", true);
            await _teamCollection.UpdateOneAsync(filter, update);
        }
}
