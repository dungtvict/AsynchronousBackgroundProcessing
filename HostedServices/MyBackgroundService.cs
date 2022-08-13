using AsynchronousBackgroundProcessing.Services;

namespace AsynchronousBackgroundProcessing.HostedServices
{
    public class MyBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MyBackgroundService> _logger;
        private readonly IMySingletonService _mySingletonService;
        private readonly IMyTransientService _myTransientService;

        public MyBackgroundService(IServiceProvider serviceProvider, ILogger<MyBackgroundService> logger,
            IMySingletonService mySingletonService, IMyTransientService myTransientService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _mySingletonService = mySingletonService;
            _myTransientService = myTransientService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedService = scope.ServiceProvider.GetRequiredService<IMyScopedService>();
            _logger.LogInformation($"{nameof(MyBackgroundService)}");
            await Task.Delay(new TimeSpan(0, 0, 1));
        }
    }
}