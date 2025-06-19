using ITRTelemetry.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DailyTaskService : BackgroundService
{
    private readonly ILogger<DailyTaskService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;

    public DailyTaskService(ILogger<DailyTaskService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyTaskService started at {time}", DateTime.Now);

        // Schedule the first execution at 5:00 PM
        ScheduleNextRun();

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Executing daily task at: {time}", DateTime.Now);

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var controller = scope.ServiceProvider.GetRequiredService<BatchComplianceGeneration>();
                controller.ProcessComplianceData();
                _logger.LogInformation("Batch Compliance Executed Successfully at {time}", DateTime.Now);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error calling Batch Compliance: {message}", ex.Message);
        }

        // Schedule the next execution
        ScheduleNextRun();
    }
    // very day at perticular time
    private void ScheduleNextRun1()
    {
        var now = DateTime.Now;
        var nextRun = now.Date.AddHours(17).AddMinutes(07); // 17:00 (5:00 PM)

        // If it's already past 5:00 PM today, schedule for tomorrow
        if (now > nextRun)
        {
            nextRun = nextRun.AddDays(1);
        }

        var delay = nextRun - now;

        _logger.LogInformation("Next execution scheduled at: {nextRun}", nextRun);

        _timer = new Timer(DoWork, null, delay, Timeout.InfiniteTimeSpan);
    }

    // every one hour
    private void ScheduleNextRun()
    {
        var now = DateTime.Now;
        var nextRun = now.AddHours(1); // Run after 1 hour

        var delay = nextRun - now;

        _logger.LogInformation("Next execution scheduled at: {nextRun}", nextRun);

        // Schedule the first run after the initial delay, then repeat every 1 hour
        _timer = new Timer(DoWork, null, delay, TimeSpan.FromHours(1));
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyTaskService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}

//public class DailyTaskService : BackgroundService
//{
//    private readonly ILogger<DailyTaskService> _logger;
//    private readonly IServiceProvider _serviceProvider; // Use IServiceProvider to create controller
//    private Timer? _timer;

//    public DailyTaskService(ILogger<DailyTaskService> logger, IServiceProvider serviceProvider)
//    {
//        _logger = logger;
//        _serviceProvider = serviceProvider; // Store service provider
//    }

//    protected override Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        await Task.Delay(5000); // Small delay to ensure application startup
//        DoWork(null);

//        // Schedule the next execution at 11:59 PM
//        ScheduleNextRun();

//        _logger.LogInformation("Service started at: {time}", DateTime.Now);

//        // Run immediately, then every 5 minutes
//        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));

//        return Task.CompletedTask;
//    }

//    private void DoWork(object? state)
//    {
//        _logger.LogInformation("Executing task at: {time}", DateTime.Now);

//        try
//        {
//            // Create a new scope to resolve scoped services
//            using (var scope = _serviceProvider.CreateScope())
//            {
//                var controller = scope.ServiceProvider.GetRequiredService<BatchComplianceGeneration>();
//                var result = controller.ProcessComplianceData(); // Call the method
//                 // _logger.LogInformation("Batch Compliance Executed Successfully at: {time}", DateTime.Now);
//                ScheduleNextRun();
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError("Error calling Batch Compliance: {message}", ex.Message);
//        }
//    }
//    private void ScheduleNextRun()
//    {
//        var now = DateTime.Now;
//        var nextRun = now.Date.AddHours(12).AddMinutes(15); // Set to 11:59 PM

//        if (now.TimeOfDay >= new TimeSpan(, 15, 0))
//        {
//            nextRun = nextRun.AddDays(1); // If past 11:59 PM, schedule for tomorrow
//        }

//        var delay = nextRun - now; // Calculate time left until 11:59 PM

//        //Console.WriteLine($"Current server time: {now}");
//        //Console.WriteLine($"Next execution scheduled at: {nextRun}");
//        //Console.WriteLine($"Task will execute in: {delay.TotalMinutes} minutes");

//        _timer = new Timer(DoWork, null, delay, Timeout.InfiniteTimeSpan);
//    }


//    public override Task StopAsync(CancellationToken stoppingToken)
//    {
//        _logger.LogInformation("Service is stopping.");
//        _timer?.Change(Timeout.Infinite, 0);
//        return Task.CompletedTask;
//    }
//}