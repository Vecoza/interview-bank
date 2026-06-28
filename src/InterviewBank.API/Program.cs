using InterviewBank.API.Data;
using InterviewBank.API.Data.Seeders;
using InterviewBank.API.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.Password.RequireDigit           = true;
    opt.Password.MinimumLength          = 8;
    opt.Password.RequireUppercase       = false;
    opt.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await TopicSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
