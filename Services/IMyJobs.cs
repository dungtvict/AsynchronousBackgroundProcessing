using AsynchronousBackgroundProcessing.Configurations;
using Hangfire;

namespace AsynchronousBackgroundProcessing.Services
{
    public interface IMyJobs
    {
        [DisableConcurrentExecution(3 * 60)]
        Task ExecuteAsync(MyJobConfiguration configuration);
    }
}