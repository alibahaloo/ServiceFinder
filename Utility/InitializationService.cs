namespace ServiceFinder.Utility
{
    public class InitializationService : BackgroundService
    {
        private readonly ILogger<InitializationService> _logger;

        public InitializationService(IServiceProvider services,
            ILogger<InitializationService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Initialization Hosted Service running.");

            await Initialize(stoppingToken);
        }

        private async Task Initialize(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Initialization Hosted Service is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<InitializeRolesService>();

                await scopedProcessingService.InitializeBaseData(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Initialization Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
