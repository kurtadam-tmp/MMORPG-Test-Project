namespace MMORPG.Domain.Interfaces;

public interface IExperienceService
{
    long GetRequiredExperienceForLevel(int level);
    (int NewLevel, long NewExperience, bool LeveledUp) GrantExperience(int currentLevel, long currentExp, long expGained);
}
