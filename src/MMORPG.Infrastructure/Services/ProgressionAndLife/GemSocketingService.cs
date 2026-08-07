using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class GemSocketingService : IGemSocketingService
{
    private readonly ConcurrentDictionary<Guid, SocketedEquipment> _items = new();

    private static readonly Dictionary<string, (int attack, int armor, int stats)> GemStats = new()
    {
        { "Flawless Ruby", (50, 0, 10) },
        { "Flawless Sapphire", (20, 20, 10) },
        { "Flawless Emerald", (40, 0, 15) },
        { "Flawless Diamond", (0, 60, 15) },
        { "Jah Rune", (30, 10, 5) },
        { "Ith Rune", (15, 15, 5) },
        { "Ber Rune", (50, 50, 10) }
    };

    public bool PunchSockets(Guid itemId, string itemName, out SocketedEquipment equipment)
    {
        equipment = _items.GetOrAdd(itemId, id => new SocketedEquipment
        {
            ItemId = id,
            ItemName = itemName,
            MaxSockets = 3,
            InsertedGems = new List<string>(),
            ActiveRuneWord = string.Empty,
            BonusAttackPower = 0,
            BonusArmor = 0,
            BonusAllStats = 0
        });

        Console.WriteLine($"[GemService PUNCH] Item '{itemName}' ({itemId}) punched with {equipment.MaxSockets} Sockets!");
        return true;
    }

    public bool InsertGemOrRune(Guid itemId, string gemOrRuneName, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (!_items.TryGetValue(itemId, out var item))
        {
            resultMessage = "Soketli eşya bulunamadı.";
            return false;
        }

        lock (item)
        {
            if (item.InsertedGems.Count >= item.MaxSockets)
            {
                resultMessage = "Eşyadaki tüm soketler dolu!";
                return false;
            }

            item.InsertedGems.Add(gemOrRuneName);
            if (GemStats.TryGetValue(gemOrRuneName, out var stats))
            {
                item.BonusAttackPower += stats.attack;
                item.BonusArmor += stats.armor;
                item.BonusAllStats += stats.stats;
            }

            // Check for Rune Word Synergy (e.g., Jah + Ith + Ber = Enigma)
            string runeSequence = string.Join(" + ", item.InsertedGems);
            if (runeSequence.Equals("Jah Rune + Ith Rune + Ber Rune", StringComparison.OrdinalIgnoreCase))
            {
                item.ActiveRuneWord = "Rune Word: Enigma";
                item.BonusAttackPower += 150;
                item.BonusAllStats += 30;
                resultMessage = $"EFSANEVİ RÜN SÖZÜ OLUŞTURULDU! '{item.ItemName}' eşyasında 'RUNE WORD: ENIGMA' (Işınlanma Becerisi & +150 Hasar) aktifleştirildi!";
                Console.WriteLine($"[GemService RUNE WORD SUCCESS] Item '{itemId}' unlocked RUNE WORD: ENIGMA!");
            }
            else
            {
                resultMessage = $"'{gemOrRuneName}' başarıyla sokete takıldı! ({item.InsertedGems.Count}/{item.MaxSockets})";
                Console.WriteLine($"[GemService INSERT] Inscribed '{gemOrRuneName}' into '{item.ItemName}'.");
            }

            return true;
        }
    }

    public bool RemoveAllGems(Guid itemId, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (_items.TryGetValue(itemId, out var item))
        {
            lock (item)
            {
                int gemCount = item.InsertedGems.Count;
                item.InsertedGems.Clear();
                item.ActiveRuneWord = string.Empty;
                item.BonusAttackPower = 0;
                item.BonusArmor = 0;
                item.BonusAllStats = 0;
                resultMessage = $"Eşyadaki {gemCount} adet mücevher/rün temizlendi.";
                Console.WriteLine($"[GemService UNSOCKET] Cleared all gems from item '{item.ItemName}'.");
                return true;
            }
        }
        return false;
    }
}
