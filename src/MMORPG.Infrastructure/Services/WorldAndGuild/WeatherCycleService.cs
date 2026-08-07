using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class WeatherCycleService : IWeatherCycleService
{
    private readonly WorldState _state = new();
    private int _tickCounter = 0;

    public WorldState GetCurrentWorldState() => _state;

    public void TickWorldTime()
    {
        _tickCounter++;
        if (_tickCounter % 300 == 0) // Every ~10 seconds toggle Weather & Day/Night
        {
            _state.IsNight = !_state.IsNight;
            _state.CurrentWeather = (WeatherType)Random.Shared.Next(0, 5);
            _state.ExpMultiplier = _state.IsNight ? 1.25f : 1.0f;
            _state.TimeOfDay = _state.IsNight ? "00:00 AM (Midnight)" : "12:00 PM (Noon)";

            Console.WriteLine($"[WeatherEngine] World State Updated -> Time: {_state.TimeOfDay}, Weather: {_state.CurrentWeather}, Night EXP Bonus: {_state.ExpMultiplier}x!");
        }
    }
}
