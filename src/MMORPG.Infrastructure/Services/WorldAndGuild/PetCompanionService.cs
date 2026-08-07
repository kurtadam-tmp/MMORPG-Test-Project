using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class PetCompanionService : IPetCompanionService
{
    private readonly ConcurrentDictionary<Guid, PetEntity> _pets = new();

    public bool TamePet(Guid ownerId, string species, out PetEntity pet)
    {
        pet = new PetEntity
        {
            PetId = Guid.NewGuid(),
            OwnerCharacterId = ownerId,
            PetName = $"Tamed {species}",
            Species = species,
            Level = 1,
            CurrentExp = 0,
            Loyalty = 100,
            EvolutionTier = "Juvenile",
            ActiveCommand = "Follow",
            BonusDamage = species.Equals("Dire Wolf", StringComparison.OrdinalIgnoreCase) ? 25 : 10,
            BonusArmor = species.Equals("Armored Bear", StringComparison.OrdinalIgnoreCase) ? 30 : 10
        };

        _pets[pet.PetId] = pet;
        Console.WriteLine($"[PetService TAME SUCCESS] Character '{ownerId}' tamed a new {species} pet '{pet.PetName}'!");
        return true;
    }

    public bool FeedPet(Guid ownerId, Guid petId, out int newLoyalty)
    {
        newLoyalty = 0;
        if (_pets.TryGetValue(petId, out var pet) && pet.OwnerCharacterId == ownerId)
        {
            pet.Loyalty = Math.Min(100, pet.Loyalty + 20);
            newLoyalty = pet.Loyalty;
            Console.WriteLine($"[PetService FEED] Pet '{pet.PetName}' fed! Loyalty increased to {pet.Loyalty}/100.");
            return true;
        }
        return false;
    }

    public bool AddPetExperience(Guid petId, long expGained, out string evolutionMessage)
    {
        evolutionMessage = string.Empty;
        if (_pets.TryGetValue(petId, out var pet))
        {
            pet.CurrentExp += expGained;
            long requiredExp = pet.Level * 250;

            if (pet.CurrentExp >= requiredExp && pet.Level < 60)
            {
                pet.Level++;
                pet.CurrentExp -= requiredExp;
                pet.BonusDamage += 5;
                pet.BonusArmor += 3;

                // Check evolution thresholds
                if (pet.Level == 20 && pet.EvolutionTier == "Juvenile")
                {
                    pet.EvolutionTier = "Adult";
                    evolutionMessage = $"Tebrikler! Evcil hayvanınız '{pet.PetName}' ADULT evresine evrimleşti!";
                    Console.WriteLine($"[PetService EVOLUTION] Pet '{pet.PetName}' EVOLVED to ADULT!");
                }
                else if (pet.Level == 40 && pet.EvolutionTier == "Adult")
                {
                    pet.EvolutionTier = "Ancient Elder";
                    pet.BonusDamage += 25;
                    evolutionMessage = $"EFSANEVİ! Evcil hayvanınız '{pet.PetName}' ANCIENT ELDER evresine evrimleşti (+25 Ek Hasar Aura)!";
                    Console.WriteLine($"[PetService EVOLUTION] Pet '{pet.PetName}' EVOLVED to ANCIENT ELDER!");
                }
                else
                {
                    evolutionMessage = $"Evcil hayvanınız '{pet.PetName}' Seviye {pet.Level}'e ulaştı!";
                }

                return true;
            }
        }
        return false;
    }

    public bool SetPetCommand(Guid petId, string command)
    {
        if (_pets.TryGetValue(petId, out var pet))
        {
            pet.ActiveCommand = command;
            Console.WriteLine($"[PetService COMMAND] Pet '{pet.PetName}' set command to '{command}'.");
            return true;
        }
        return false;
    }

    public List<PetEntity> GetPetsForCharacter(Guid ownerId)
    {
        return _pets.Values.Where(p => p.OwnerCharacterId == ownerId).ToList();
    }
}
