
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


var authority = builder.Configuration["Jwt:Authority"];
var audience = builder.Configuration["Jwt:Audience"];
var secret = builder.Configuration["Jwt:Secret"];
var connectionString = builder.Configuration.GetConnectionString("Default");

Console.WriteLine($"Authority = {authority}");
Console.WriteLine($"Audience  = {audience}");
Console.WriteLine($"Connection String = {connectionString}");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = authority;
    // Helpful in dev if there’s a tiny clock drift
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        // Issuer
        ValidateIssuer = true,
        ValidIssuers = new[]
        {
            authority.TrimEnd('/'),
            authority.TrimEnd('/') + "/"
        },

        // Audience
        ValidateAudience = true,
        ValidAudience = audience, // "authenticated"

        // Lifetime & signature
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),
    };

    options.IncludeErrorDetails = true;
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
