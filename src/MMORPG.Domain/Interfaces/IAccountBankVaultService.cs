namespace MMORPG.Domain.Interfaces;

public class AccountBankVault
{
    public Guid AccountId { get; set; }
    public long VaultGold { get; set; }
    public int MaxVaultSlots { get; set; } = 20;
    public List<string> VaultItemNames { get; set; } = new();
}

public interface IAccountBankVaultService
{
    AccountBankVault GetVaultForAccount(Guid accountId);
    bool DepositGold(Guid accountId, long amount, out long newBalance);
    bool WithdrawGold(Guid accountId, long amount, out long newBalance);
    bool DepositItem(Guid accountId, string itemName, out string resultMessage);
    bool WithdrawItem(Guid accountId, string itemName, out string resultMessage);
    bool ExpandVaultSlots(Guid accountId, out int newMaxSlots);
}
