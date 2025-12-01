
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var authority = builder.Configuration["Jwt:Authority"];
var audience = builder.Configuration["Jwt:Audience"];
var secret = builder.Configuration["Jwt:Secret"];
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = authority;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidIssuers = new[] { authority },

        ValidateAudience = true,
        ValidAudience = audience, 

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),
    };

    if (builder.Environment.IsDevelopment())
    {
        options.IncludeErrorDetails = true;
    }

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("OnMessageReceived invoked");
            var accessToken = context.Request.Query["access_token"];
            logger.LogInformation("Access Token: {AccessToken}", accessToken);
            var path = context.HttpContext.Request.Path;
            logger.LogInformation("Request Path: {Path}", path);

            // MUST match your hub path exactly: /api/chat
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/chat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            if (context.Principal == null)
            {
                logger.LogError("Principal is null");
                throw new Exception("Principal is null");
            }

            var AuthService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
            var dbUser = await AuthService.GetAuthUser(context.Principal); 

            context.HttpContext.Items["CurrentUser"] = dbUser;
        }
    };
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddSingleton<IUserIdProvider, EmailUserIdProvider>();

builder.Services.AddSignalR(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/api/chat");

app.Run();
