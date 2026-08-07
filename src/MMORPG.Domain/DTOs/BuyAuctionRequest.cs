namespace MMORPG.Domain.DTOs;

public class BuyAuctionRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid BuyerCharacterId { get; set; }
    public Guid ListingId { get; set; }
}
