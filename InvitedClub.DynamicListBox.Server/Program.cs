using InvitedClub.DynamicListBox.Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCors", policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin() 
    );
});

builder.Services.AddDbContext<ListBoxDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ListBoxDb")));

var app = builder.Build();

//Ensure the database is created, migrations are applied, and initial list box items are seeded for local development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ListBoxDbContext>();
    db.Database.Migrate();

    if (!db.Items.Any())
    {
        db.Items.AddRange(
            new ListBoxItemEntity { Text = "Golf", SortOrder = 1 },
            new ListBoxItemEntity { Text = "Tennisb", SortOrder = 2 },
            new ListBoxItemEntity { Text = "Pickleball", SortOrder = 3 },
            new ListBoxItemEntity { Text = "Swimming", SortOrder = 4 },
            new ListBoxItemEntity { Text = "Yoga & Pilates", SortOrder = 5 },
            new ListBoxItemEntity { Text = "Fitness Center", SortOrder = 6 }
            );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ClientCors");

app.UseAuthorization();

app.MapControllers();

app.Run();
