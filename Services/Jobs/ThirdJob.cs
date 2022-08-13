using AsynchronousBackgroundProcessing.Configurations;

namespace AsynchronousBackgroundProcessing.Services.Jobs
{
    public class ThirdJob : IThirdJob
    {
        private readonly ILogger<ThirdJob> _logger;

        public ThirdJob(ILogger<ThirdJob> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(MyJobConfiguration configuration)
        {
            _logger.LogInformation($"{configuration.BackgroundJobId} is starting...");
            // TODO: Implement logic...
            await Task.Delay(new TimeSpan(0, 0, 3));
            _logger.LogInformation($"{configuration.BackgroundJobId} is completed");
        }
    }
}