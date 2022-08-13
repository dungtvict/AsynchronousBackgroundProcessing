using AsynchronousBackgroundProcessing.Configurations;

namespace AsynchronousBackgroundProcessing.Services.Jobs
{
    public class SecondJob : ISecondJob
    {
        private readonly ILogger<SecondJob> _logger;

        public SecondJob(ILogger<SecondJob> logger)
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