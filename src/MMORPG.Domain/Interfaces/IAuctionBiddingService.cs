namespace MMORPG.Domain.Interfaces;

public class AuctionBidItem
{
    public Guid ListingId { get; set; } = Guid.NewGuid();
    public Guid SellerCharacterId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public long StartingBidGold { get; set; }
    public long CurrentBidGold { get; set; }
    public Guid CurrentHighestBidderId { get; set; }
    public long BuyoutGold { get; set; }
    public DateTime ExpirationTime { get; set; }
    public bool IsSold { get; set; }
    public bool IsExpired { get; set; }
}

public interface IAuctionBiddingService
{
    AuctionBidItem CreateListing(Guid sellerId, string itemName, long startingBid, long buyout, int durationHours);
    bool PlaceBid(Guid bidderId, Guid listingId, long bidAmount, out string resultMessage);
    bool BuyoutListing(Guid buyerId, Guid listingId, out string resultMessage);
    List<AuctionBidItem> ProcessExpiredListings();
}
