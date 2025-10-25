using TaskManagementSystem.Context;
using TaskManagementSystem.Helper;
using TaskManagementSystem.Middlewares;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;


var builder = WebApplication.CreateBuilder(args);





// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TaskManagementDBSettings>(
    builder.Configuration.GetSection("TaskManagementDB"));

builder.Services.AddSingleton<MongoDbContext>();



builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<AccessHelper>();
builder.Services.AddScoped<Jwthelper>();
builder.Services.AddScoped<AuthMiddleware>();


//set-up cors:
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});



var app = builder.Build();






// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthentication();

app.MapControllers();

app.Run();
