namespace MMORPG.Domain.Entities;

public class AuctionListing
{
    public Guid Id { get; set; }
    public Guid SellerCharacterId { get; set; }
    public Guid ItemInstanceId { get; set; }
    public long PriceGold { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, SOLD, CANCELLED, EXPIRED
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}
