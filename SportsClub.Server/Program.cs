using SportsClub.Server.Data;
using SportsClub.Server;
using SportsClub.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS is restricted to the origins configured under "Cors:AllowedOrigins".
// No origins are allowed unless they are configured (secure by default).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCors", policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddDbContext<ListBoxDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ListBoxDb")));

// Application services
builder.Services.AddScoped<IListBoxItemService, ListBoxItemService>();
builder.Services.AddScoped<IListBoardService, ListBoardService>();

// Global error handling -> RFC-7807 ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

//Ensure the database is created, migrations are applied, and sample boards + items are seeded for local development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ListBoxDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();

    if (!db.Boards.Any())
    {
        db.Boards.AddRange(
            new ListBoardEntity
            {
                Name = "Racquet Sports",
                Items =
                {
                    new ListBoxItemEntity { Text = "Tennis", SortOrder = 1 },
                    new ListBoxItemEntity { Text = "Pickleball", SortOrder = 2 },
                    new ListBoxItemEntity { Text = "Squash", SortOrder = 3 }
                }
            },
            new ListBoardEntity
            {
                Name = "Aquatics",
                Items =
                {
                    new ListBoxItemEntity { Text = "Lap Swimming", SortOrder = 1 },
                    new ListBoxItemEntity { Text = "Water Aerobics", SortOrder = 2 }
                }
            },
            new ListBoardEntity
            {
                Name = "Fitness & Wellness",
                Items =
                {
                    new ListBoxItemEntity { Text = "Yoga & Pilates", SortOrder = 1 },
                    new ListBoxItemEntity { Text = "Fitness Center", SortOrder = 2 },
                    new ListBoxItemEntity { Text = "Spin Studio", SortOrder = 3 }
                }
            });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("ClientCors");

// NOTE: No authentication/authorization is configured for this reference app.
// Add builder.Services.AddAuthentication()/AddAuthorization() and the matching
// app.UseAuthentication()/UseAuthorization() middleware here when it is needed.

app.MapControllers();

app.Run();

public partial class Program { }
