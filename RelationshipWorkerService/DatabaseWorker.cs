using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RelationshipWorkerService.Entities;
using RelationshipWorkerService.Tools;

namespace RelationshipWorkerService
{
    public class DatabaseWorker
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        FileLogger fileLogger = new FileLogger();

        public DatabaseWorker(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CheckDatabaseAsync(CancellationToken cancellationToken)
        {
            fileLogger.writeToLog("====Checking database for updates====");
            // Create a short-lived DbContext instance per operation
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            // Add an entry
            context.Prompts.Add(new Prompt
            {
                PromptText = "This is a sample prompt text."
            });

            await context.SaveChangesAsync(cancellationToken);

            // Query entries asynchronously
            var recentLogs = await context.Prompts
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

            foreach (var log in recentLogs)
            {
                fileLogger.writeToLog($"[{log.CreatedAt}] {log.PromptText}");
            }

            fileLogger.writeToLog("====Database check complete====");

        }
    }
}