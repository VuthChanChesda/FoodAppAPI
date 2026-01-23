using FoodAppAPI.Data;
using FoodAppAPI.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// SERVICE REGISTRATIONS ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

// Helpers & Settings
builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddScoped<PhotoHelper>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Background Workers
builder.Services.AddHostedService<ExpiredItemWorker>();
builder.Services.AddHostedService<ExpiryMonitorService>();

// External APIs (Dio equivalent in C#)
builder.Services.AddHttpClient<OpenRouterService>();
builder.Services.AddHttpClient<HuggingFaceService>();
builder.Services.AddHttpClient<GeminiAiService>();



builder.Services.AddDbContext<foodAppContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

//  MIDDLEWARE PIPELINE (ORDER MATTERS!) ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//Authentication comes first
app.UseAuthentication();

// Authorization comes second
app.UseAuthorization();

// Map Endpoints (Controllers & Hubs)

app.MapControllers();
app.MapHub<ExpiryHub>("/expiryHub");

app.Run();
