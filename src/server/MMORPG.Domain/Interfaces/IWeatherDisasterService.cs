namespace MMORPG.Domain.Interfaces;

public class WeatherDisasterEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");
    public string DisasterType { get; set; } = string.Empty;
    public int AffectedZoneId { get; set; }
    public string GlobalBuffName { get; set; } = string.Empty;
    public int MovementSpeedModifierPercent { get; set; }
    public int DamageBonusPercent { get; set; }
    public DateTime ExpirationTime { get; set; }
}

public interface IWeatherDisasterService
{
    WeatherDisasterEvent TriggerDisaster(string disasterType, int zoneId, int durationMinutes);
    WeatherDisasterEvent TriggerCelestialAlignment(string alignmentType, int durationMinutes);
    WeatherDisasterEvent GetActiveDisasterForZone(int zoneId);
}
