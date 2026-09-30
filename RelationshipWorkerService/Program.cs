using Microsoft.EntityFrameworkCore;
using RelationshipWorkerService;
using RelationshipWorkerService.Entities;

var builder = Host.CreateApplicationBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");


builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailConnection"));

builder.Services.AddTransient<DatabaseWorker>();
builder.Services.AddTransient<EmailSystem>();


builder.Services.AddWindowsService();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();