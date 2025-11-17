
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


var authority = builder.Configuration["Jwt:Authority"];
var audience = builder.Configuration["Jwt:Audience"];
var secret = builder.Configuration["Jwt:Secret"];
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = authority;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidIssuers = new[]
        {
            authority.TrimEnd('/'),
            authority.TrimEnd('/') + "/"
        },

        ValidateAudience = true,
        ValidAudience = audience, 

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),
    };

    options.IncludeErrorDetails = true;

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            if (context.Principal == null)
            {
                throw new Exception("Principal is null");
            }

            var AuthService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
            var tokenData = AuthService.GetTokenDataFromContext(context.Principal); 

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            if (tokenData.issuer == null || tokenData.subject == null)
            {
                throw new Exception("Issuer or Subject claim is missing");
            }
            logger.LogInformation("Ensuring user exists in database...");
            var UserService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
            await UserService.EnsureUserExistsAsync(tokenData.issuer, tokenData.subject);
            logger.LogInformation("User existence ensured.");
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
