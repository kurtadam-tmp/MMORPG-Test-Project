using Microsoft.Extensions.Hosting;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Server.Engine;

public class WriteBehindFlushWorker : BackgroundService
{
    private readonly IWriteBehindService _writeBehindService;
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(30);

    public WriteBehindFlushWorker(IWriteBehindService writeBehindService)
    {
        _writeBehindService = writeBehindService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"[WriteBehindWorker] Initialized with interval {_flushInterval.TotalSeconds} seconds.");

        using var timer = new PeriodicTimer(_flushInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var count = await _writeBehindService.FlushDirtyCharactersAsync();
                if (count > 0)
                {
                    Console.WriteLine($"[WriteBehindWorker] Flushed {count} dirty character state(s) to PostgreSQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteBehindWorker Error] Failed to flush state: {ex.Message}");
            }
        }
    }
}
