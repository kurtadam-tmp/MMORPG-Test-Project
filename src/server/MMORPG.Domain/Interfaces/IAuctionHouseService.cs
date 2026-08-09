using MMORPG.Domain.DTOs;

namespace MMORPG.Domain.Interfaces;

public interface IAuctionHouseService
{
    Task<AuctionResult> CreateListingAsync(CreateAuctionRequest request);
    Task<AuctionResult> BuyItemAsync(BuyAuctionRequest request);
    Task<AuctionResult> GetActiveListingsAsync();
    Task<AuctionResult> CancelListingAsync(string sessionToken, Guid sellerCharacterId, Guid listingId);
}
