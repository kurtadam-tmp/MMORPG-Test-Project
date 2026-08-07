using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AccountBankVaultService : IAccountBankVaultService
{
    private readonly ConcurrentDictionary<Guid, AccountBankVault> _vaults = new();

    public AccountBankVault GetVaultForAccount(Guid accountId)
    {
        return _vaults.GetOrAdd(accountId, id => new AccountBankVault
        {
            AccountId = id,
            VaultGold = 1000,
            MaxVaultSlots = 20,
            VaultItemNames = new List<string>()
        });
    }

    public bool DepositGold(Guid accountId, long amount, out long newBalance)
    {
        newBalance = 0;
        var vault = GetVaultForAccount(accountId);
        lock (vault)
        {
            if (amount <= 0) return false;
            vault.VaultGold += amount;
            newBalance = vault.VaultGold;
            Console.WriteLine($"[BANK VAULT DEPOSIT GOLD] Account '{accountId}' deposited {amount} Gold. Total Vault Balance: {newBalance} Gold.");
            return true;
        }
    }

    public bool WithdrawGold(Guid accountId, long amount, out long newBalance)
    {
        newBalance = 0;
        var vault = GetVaultForAccount(accountId);
        lock (vault)
        {
            if (amount <= 0 || vault.VaultGold < amount) return false;
            vault.VaultGold -= amount;
            newBalance = vault.VaultGold;
            Console.WriteLine($"[BANK VAULT WITHDRAW GOLD] Account '{accountId}' withdrew {amount} Gold. Remaining Vault Balance: {newBalance} Gold.");
            return true;
        }
    }

    public bool DepositItem(Guid accountId, string itemName, out string resultMessage)
    {
        resultMessage = string.Empty;
        var vault = GetVaultForAccount(accountId);
        lock (vault)
        {
            if (vault.VaultItemNames.Count >= vault.MaxVaultSlots)
            {
                resultMessage = "Hesap Kasası dolu! Daha fazla yuva açmanız gerekiyor.";
                return false;
            }

            vault.VaultItemNames.Add(itemName);
            resultMessage = $"'{itemName}' eşyası Hesap Kasasına yatırıldı! ({vault.VaultItemNames.Count}/{vault.MaxVaultSlots})";
            Console.WriteLine($"[BANK VAULT DEPOSIT ITEM] Account '{accountId}' stored '{itemName}' in vault.");
            return true;
        }
    }

    public bool WithdrawItem(Guid accountId, string itemName, out string resultMessage)
    {
        resultMessage = string.Empty;
        var vault = GetVaultForAccount(accountId);
        lock (vault)
        {
            if (!vault.VaultItemNames.Contains(itemName))
            {
                resultMessage = "Eşya kasada bulunamadı.";
                return false;
            }

            vault.VaultItemNames.Remove(itemName);
            resultMessage = $"'{itemName}' eşyası Hesap Kasasından çekildi!";
            Console.WriteLine($"[BANK VAULT WITHDRAW ITEM] Account '{accountId}' retrieved '{itemName}' from vault.");
            return true;
        }
    }

    public bool ExpandVaultSlots(Guid accountId, out int newMaxSlots)
    {
        newMaxSlots = 0;
        var vault = GetVaultForAccount(accountId);
        lock (vault)
        {
            if (vault.MaxVaultSlots >= 100) return false;
            vault.MaxVaultSlots += 10;
            newMaxSlots = vault.MaxVaultSlots;
            Console.WriteLine($"[BANK VAULT EXPAND] Account '{accountId}' expanded vault slots to {newMaxSlots}!");
            return true;
        }
    }
}
