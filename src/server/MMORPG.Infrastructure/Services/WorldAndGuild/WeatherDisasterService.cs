using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class WeatherDisasterService : IWeatherDisasterService
{
    private readonly ConcurrentDictionary<int, WeatherDisasterEvent> _zoneDisasters = new();

    public WeatherDisasterEvent TriggerDisaster(string disasterType, int zoneId, int durationMinutes)
    {
        var disaster = new WeatherDisasterEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            DisasterType = disasterType,
            AffectedZoneId = zoneId,
            ExpirationTime = DateTime.UtcNow.AddMinutes(durationMinutes)
        };

        if (disasterType.Equals("Volcanic Eruption", StringComparison.OrdinalIgnoreCase))
        {
            disaster.GlobalBuffName = "Alev Felaketi (+20% Ateş Hasarı, Lava Dalgası)";
            disaster.MovementSpeedModifierPercent = -20;
            disaster.DamageBonusPercent = 20;
        }
        else if (disasterType.Equals("Blizzard Tsunami", StringComparison.OrdinalIgnoreCase))
        {
            disaster.GlobalBuffName = "Buz Kasırgası (+25% Buz Hasarı, %40 Yavaşlatma)";
            disaster.MovementSpeedModifierPercent = -40;
            disaster.DamageBonusPercent = 25;
        }
        else
        {
            disaster.GlobalBuffName = "Fırtına Etkinliği (+30% Saldırı Hızı)";
            disaster.MovementSpeedModifierPercent = 10;
            disaster.DamageBonusPercent = 15;
        }

        _zoneDisasters[zoneId] = disaster;
        Console.WriteLine($"[WEATHER DISASTER TRIGGERED!] '{disasterType}' activated in Zone #{zoneId}! ({disaster.GlobalBuffName}, Duration: {durationMinutes}m)");
        return disaster;
    }

    public WeatherDisasterEvent TriggerCelestialAlignment(string alignmentType, int durationMinutes)
    {
        var alignment = new WeatherDisasterEvent
        {
            EventId = Guid.NewGuid().ToString("N"),
            DisasterType = alignmentType,
            AffectedZoneId = 0, // World-wide
            GlobalBuffName = alignmentType.Equals("Solar Eclipse", StringComparison.OrdinalIgnoreCase) ? "Güneş Tutulması (+30% Fiziksel Hasar & %10 Lifesteal)" : "Ay Tutulması (+30% Büyü Hasarı & %50 Mana Yenilenmesi)",
            MovementSpeedModifierPercent = 15,
            DamageBonusPercent = 30,
            ExpirationTime = DateTime.UtcNow.AddMinutes(durationMinutes)
        };

        _zoneDisasters[0] = alignment;
        Console.WriteLine($"[GLOBAL CELESTIAL ALIGNMENT!] '{alignmentType}' activated WORLD-WIDE! ({alignment.GlobalBuffName})");
        return alignment;
    }

    public WeatherDisasterEvent GetActiveDisasterForZone(int zoneId)
    {
        if (_zoneDisasters.TryGetValue(zoneId, out var disaster) && DateTime.UtcNow < disaster.ExpirationTime)
        {
            return disaster;
        }

        // Check world-wide celestial alignment
        if (_zoneDisasters.TryGetValue(0, out var globalAlignment) && DateTime.UtcNow < globalAlignment.ExpirationTime)
        {
            return globalAlignment;
        }

        return null!;
    }
}
