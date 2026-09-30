using Microsoft.EntityFrameworkCore;
using RelationshipWorkerService.Tools;

namespace RelationshipWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly EmailSystem _emailSystem;
        //private readonly DatabaseWorker _dbWorker;
        FileLogger fileLogger = new FileLogger();

        // DatabaseWorker dbWorker,
        public Worker(EmailSystem emailSystem, IDbContextFactory<AppDbContext> contextFactory)
        {
            _emailSystem = emailSystem;
            //_dbWorker = dbWorker;
            _contextFactory = contextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                fileLogger.writeToLog("Starting program...");

                try
                {
                    await _emailSystem.RunEmailSystem(stoppingToken);
                    //await _emailSystem.GetPromptFromApi(stoppingToken);

                    await Task.Delay(10000, stoppingToken);
                }
                catch (Exception exception)
                {
                    fileLogger.writeToLog(exception.ToString());
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
}