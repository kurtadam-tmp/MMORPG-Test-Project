namespace MMORPG.Domain.Interfaces;

public enum WeatherType
{
    Clear = 0,
    Rain = 1,
    Fog = 2,
    Storm = 3,
    Blizzard = 4,
    SolarEclipse = 5
}

public class WorldState
{
    public bool IsNight { get; set; }
    public WeatherType CurrentWeather { get; set; } = WeatherType.Clear;
    public float ExpMultiplier { get; set; } = 1.0f;
    public string TimeOfDay { get; set; } = "12:00 PM (Noon)";
}

public interface IWeatherCycleService
{
    WorldState GetCurrentWorldState();
    void TickWorldTime();
}
