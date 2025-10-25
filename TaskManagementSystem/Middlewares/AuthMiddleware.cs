using MongoDB.Bson;
using System.Text;
using System.Text.Json;
using TaskManagementSystem.Helper;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;
using System.Text.Json;

namespace TaskManagementSystem.Middlewares
{

    public class User {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }


    public class AuthMiddleware : IMiddleware
        {

            private readonly Jwthelper _jwtHelper;
            private readonly UserService _userService;
            private readonly AccessHelper _accessHelper;

            public AuthMiddleware(Jwthelper jwthelper, UserService userService,AccessHelper accessHelper)
            {

                _jwtHelper = jwthelper;
                _userService = userService;
                _accessHelper = accessHelper;
                 
            }





            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {

            if (!IsAdminAccessRoute(context) && !IsTeamLeadAccessRoute(context) && !IsActiveUserCheckRoute(context))
            {
                await next(context);
                return;
            }





            if (IsActiveUserCheckRoute(context))
            {

                context.Request.EnableBuffering();
                var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;



                User userdata = JsonSerializer.Deserialize<User>(requestBody.ToString());
                //Console.WriteLine($"{user1.Email}");

                var user1 = await _userService.GetUserByEmail(userdata.Email);
                //Console.WriteLine($"inside3 {user1.Id}");
                if ((int)user1.Status == 1)
                {

                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync("Access Denied:User In-Active!");
                    return;
                }
                else
                {
                    await next(context);
                    return;

                }
            }



            var token = context.Request.Cookies["Jwt-token"];

                if (token == null)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Token Not Present" });
                    return;
                }

                var email = _jwtHelper.getJWTTokenClaim(token);

                var user = await _userService.GetUserByEmail(email!);
                int role = await _accessHelper.checkRoleAccess(user.Id!);


           

            if (user is null)
                {

                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync("Meassge:access denied");
                    return;


                }

            else if (IsTeamLeadAccessRoute(context))
            {
                if (role == 2 || role == 1) await next(context);
                else
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync("Meassge:TeamLead or Admin Access Needed");
                    return;
                }
            }

            else if (role != 1)
            {

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync("Meassge:Admin Access Needed");
                return;

            }

           

            else
            {
                await next(context);
            }


            }







        public bool IsActiveUserCheckRoute(HttpContext context)
        {
            List<string> activeStatusRoutes = new List<string>{
                                                "/Api/User/signin",
                                                          };
            if (context.Request.Path.Value is not null)
            {
                return activeStatusRoutes.Contains(context.Request.Path.Value);
            }

            return false;


        }


        
         public bool IsAdminAccessRoute(HttpContext context)
            {
                List<string> adminAccessRoutes = new List<string>{
                                                "/Api/Team/createTeam",
                                                "/Api/User/AddRole"
                                                          };

            if (context.Request.Path.Value is not null)
            {
                return adminAccessRoutes.Contains(context.Request.Path.Value);
            }

                return false;
            }


        public bool IsTeamLeadAccessRoute(HttpContext context)
        {
            List<string> teamleadAccessRoutes = new List<string>{
                                                         "/Api/Team/AssignMember",

                                                          };

            if (context.Request.Path.Value is not null)
            {
                return teamleadAccessRoutes.Contains(context.Request.Path.Value);
            }

            return false;
        }

    }



    }
