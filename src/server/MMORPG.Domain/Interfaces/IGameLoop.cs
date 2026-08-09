namespace MMORPG.Domain.Interfaces;

public interface IGameLoop
{
    int TargetTickRate { get; }
    bool IsRunning { get; }
    event Action<long, float>? OnTick; // (CurrentTickNumber, DeltaTimeInSeconds)

    Task StartAsync(CancellationToken cancellationToken);
    void Stop();
}
