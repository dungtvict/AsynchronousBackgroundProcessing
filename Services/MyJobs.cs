using AsynchronousBackgroundProcessing.Configurations;
using AsynchronousBackgroundProcessing.Extensions;
using AsynchronousBackgroundProcessing.Services.Jobs;

namespace AsynchronousBackgroundProcessing.Services
{
    public class MyJobs : IMyJobs
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MyJobs> _logger;

        public MyJobs(IServiceProvider serviceProvider, ILogger<MyJobs> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync(MyJobConfiguration configuration)
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var job = serviceProvider.GetService<IBaseJob>(configuration.JobCode);
            await job.ExecuteAsync(configuration);
        }
    }
}