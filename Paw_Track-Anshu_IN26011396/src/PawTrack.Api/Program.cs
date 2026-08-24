using Microsoft.EntityFrameworkCore;
using PawTrack.Core.Interfaces;
using PawTrack.Data;
using PawTrack.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework Core with In-Memory or relational database
builder.Services.AddDbContext<PawTrackDbContext>(options =>
{
    options.UseInMemoryDatabase("PawTrackDb");
});

// Register Core Domain Services (ASP.NET Core DI)
builder.Services.AddScoped<IAdoptionService, AdoptionService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IFollowUpService, FollowUpService>();
builder.Services.AddScoped<IVisitService, VisitService>();

// CORS for Blazor / Frontend client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Seed initial database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PawTrackDbContext>();
    DbInitializer.Initialize(db);
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
