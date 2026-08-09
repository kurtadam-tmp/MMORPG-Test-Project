using System.Diagnostics;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Server.Engine;

public class GameLoop : IGameLoop
{
    public int TargetTickRate { get; }
    public bool IsRunning { get; private set; }

    public event Action<long, float>? OnTick;

    private readonly double _targetFrameTimeMs;
    private long _currentTick = 0;

    public GameLoop(int targetTickRate = 30)
    {
        TargetTickRate = targetTickRate > 0 ? targetTickRate : 30;
        _targetFrameTimeMs = 1000.0 / TargetTickRate;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IsRunning = true;
        _currentTick = 0;

        var stopwatch = Stopwatch.StartNew();
        double previousTimeMs = stopwatch.Elapsed.TotalMilliseconds;

        Console.WriteLine($"[GameLoop] Started at {TargetTickRate} Hz ({_targetFrameTimeMs:F2} ms/tick).");

        while (IsRunning && !cancellationToken.IsCancellationRequested)
        {
            double currentTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            float deltaTime = (float)((currentTimeMs - previousTimeMs) / 1000.0);
            previousTimeMs = currentTimeMs;

            _currentTick++;

            try
            {
                OnTick?.Invoke(_currentTick, deltaTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLoop Error] Exception on Tick {_currentTick}: {ex.Message}");
            }

            double frameTimeMs = stopwatch.Elapsed.TotalMilliseconds - currentTimeMs;
            double sleepTimeMs = _targetFrameTimeMs - frameTimeMs;

            if (sleepTimeMs > 0)
            {
                await Task.Delay((int)sleepTimeMs, cancellationToken);
            }
        }

        IsRunning = false;
        Console.WriteLine("[GameLoop] Stopped.");
    }

    public void Stop()
    {
        IsRunning = false;
    }
}
