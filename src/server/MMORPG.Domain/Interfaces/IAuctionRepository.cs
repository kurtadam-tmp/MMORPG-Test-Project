using MMORPG.Domain.Entities;

namespace MMORPG.Domain.Interfaces;

public interface IAuctionRepository
{
    Task<AuctionListing?> GetByIdAsync(Guid listingId);
    Task<IEnumerable<AuctionListing>> GetActiveListingsAsync();
    Task<Guid> CreateListingAsync(AuctionListing listing);
    Task<bool> ExecuteAtomicPurchaseTransactionAsync(Guid listingId, Guid buyerCharacterId, long priceGold);
    Task<bool> CancelListingAsync(Guid listingId, Guid sellerCharacterId);
}
