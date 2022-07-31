using RPA.StreamingNotification.Handler;

namespace RPA.StreamingNotification
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private RequestHandler _requestHandler;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _requestHandler = new RequestHandler(logger, configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _requestHandler.CheckStatusStreaming("deabraba");
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}