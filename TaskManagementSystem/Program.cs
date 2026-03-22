using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManagementSystem.Context;
using TaskManagementSystem.Helper;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["JWT:Key"]
    ?? throw new InvalidOperationException("JWT:Key is not configured.");
var jwtIssuer = builder.Configuration["JWT:issuer"]
    ?? throw new InvalidOperationException("JWT:issuer is not configured.");
var jwtAudience = builder.Configuration["JWT:audience"]
    ?? throw new InvalidOperationException("JWT:audience is not configured.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. App clients: X-TASKLINKER-CLIENT: app and Authorization: Bearer {token}. Web: Jwt-token cookie.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<TaskManagementDBSettings>(
    builder.Configuration.GetSection("TaskManagementDB"));

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<AccessHelper>();
builder.Services.AddScoped<Jwthelper>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Email
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var client = context.Request.Headers["X-TASKLINKER-CLIENT"].FirstOrDefault();
            if (string.Equals(client, "app", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            var cookieToken = context.Request.Cookies["Jwt-token"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                context.Token = cookieToken;
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder("Bearer")
        .RequireAuthenticatedUser()
        .Build();
});

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
