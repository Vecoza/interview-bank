using System.Text;
using InterviewBank.API.Data;
using InterviewBank.API.Data.Seeders;
using InterviewBank.API.Entities;
using InterviewBank.API.Middleware;
using InterviewBank.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit           = true;
    opt.Password.RequiredLength          = 8;
    opt.Password.RequireUppercase       = false;
    opt.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Bearer ────────────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer   = false,
            ValidateAudience = false,
            ClockSkew        = TimeSpan.Zero   // access tokens expire exactly at 15 min
        };
    });

// ── CORS — allow Angular dev server + Vercel prod domain ─────────────────────
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.WithOrigins(
        builder.Configuration["AllowedOrigin"] ?? "http://localhost:4200")
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));   // required for HttpOnly refresh-token cookie

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();

// ── API / OpenAPI ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Startup tasks ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await TopicSeeder.SeedAsync(scope.ServiceProvider);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
