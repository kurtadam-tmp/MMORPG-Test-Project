namespace MMORPG.Domain.Interfaces;

public class PetEntity
{
    public Guid PetId { get; set; } = Guid.NewGuid();
    public Guid OwnerCharacterId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public long CurrentExp { get; set; }
    public int Loyalty { get; set; } = 100;
    public string EvolutionTier { get; set; } = "Juvenile";
    public string ActiveCommand { get; set; } = "Follow";
    public int BonusDamage { get; set; }
    public int BonusArmor { get; set; }
}

public interface IPetCompanionService
{
    bool TamePet(Guid ownerId, string species, out PetEntity pet);
    bool FeedPet(Guid ownerId, Guid petId, out int newLoyalty);
    bool AddPetExperience(Guid petId, long expGained, out string evolutionMessage);
    bool SetPetCommand(Guid petId, string command);
    List<PetEntity> GetPetsForCharacter(Guid ownerId);
}
