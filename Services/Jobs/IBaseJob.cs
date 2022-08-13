using AsynchronousBackgroundProcessing.Configurations;

namespace AsynchronousBackgroundProcessing.Services.Jobs
{
    public interface IBaseJob
    {
        Task ExecuteAsync(MyJobConfiguration configuration);
    }
}