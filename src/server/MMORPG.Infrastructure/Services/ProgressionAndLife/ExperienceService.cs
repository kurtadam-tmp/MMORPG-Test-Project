using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class ExperienceService : IExperienceService
{
    private const int MaxLevel = 60;

    public long GetRequiredExperienceForLevel(int level)
    {
        if (level < 1) return 100;
        if (level >= MaxLevel) return long.MaxValue;

        // Exponential level curve formula: Level^2.5 * 100
        return (long)(Math.Pow(level, 2.5) * 100);
    }

    public (int NewLevel, long NewExperience, bool LeveledUp) GrantExperience(int currentLevel, long currentExp, long expGained)
    {
        long totalExp = currentExp + expGained;
        int level = currentLevel;
        bool leveledUp = false;

        while (level < MaxLevel)
        {
            long reqExp = GetRequiredExperienceForLevel(level);
            if (totalExp >= reqExp)
            {
                level++;
                leveledUp = true;
            }
            else
            {
                break;
            }
        }

        return (level, totalExp, leveledUp);
    }
}
