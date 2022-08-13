using AsynchronousBackgroundProcessing.Configurations;
using AsynchronousBackgroundProcessing.Services;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Options;

namespace AsynchronousBackgroundProcessing.HostedServices
{
    public class MyHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly List<MyJobConfiguration>? _options;

        private readonly IMySingletonService _mySingletonService;
        private readonly IMyTransientService _myTransientService;

        private string _prefix { get; set; }

        public MyHostedService(IServiceProvider serviceProvider, IRecurringJobManager recurringJobManager,
            IOptions<List<MyJobConfiguration>> options,
            IMySingletonService mySingletonService, IMyTransientService myTransientService)
        {
            _serviceProvider = serviceProvider;
            _recurringJobManager = recurringJobManager;
            _options = options.Value;

            _mySingletonService = mySingletonService;
            _myTransientService = myTransientService;

            _prefix = Guid.NewGuid().ToString();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options == null || !_options.Any())
            {
                return Task.CompletedTask;
            }

            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var scopedService = serviceProvider.GetRequiredService<IMyScopedService>();

            foreach (MyJobConfiguration configuration in _options)
            {
                string backgroundJobId =
                    $"{_prefix}_{configuration.JobCode}";
                configuration.BackgroundJobId = backgroundJobId;
                _recurringJobManager.AddOrUpdate<IMyJobs>(backgroundJobId,
                    x => x.ExecuteAsync(configuration),
                    configuration.CronExpression);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var scopedService = serviceProvider.GetRequiredService<IMyScopedService>();
            IStorageConnection? storageConnection = serviceProvider.GetService<IStorageConnection>();
            List<RecurringJobDto> recurringJobs = storageConnection.GetRecurringJobs();
            recurringJobs = recurringJobs.Where(x => x.Id.StartsWith(_prefix)).ToList();
            foreach (var recurringJob in recurringJobs)
            {
                _recurringJobManager.RemoveIfExists(recurringJob.Id);
            }

            return Task.CompletedTask;
        }
    }
}