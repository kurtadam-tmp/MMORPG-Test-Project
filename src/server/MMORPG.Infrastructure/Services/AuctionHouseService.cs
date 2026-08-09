using MMORPG.Domain.DTOs;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AuctionHouseService : IAuctionHouseService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPlayerSessionService _sessionService;

    public AuctionHouseService(
        IAuctionRepository auctionRepository,
        IInventoryRepository inventoryRepository,
        IPlayerSessionService sessionService)
    {
        _auctionRepository = auctionRepository;
        _inventoryRepository = inventoryRepository;
        _sessionService = sessionService;
    }

    public async Task<AuctionResult> CreateListingAsync(CreateAuctionRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.CharacterId)
        {
            return new AuctionResult { Success = false, Message = "Unauthorized session." };
        }

        if (request.PriceGold <= 0)
        {
            return new AuctionResult { Success = false, Message = "Price must be greater than zero gold." };
        }

        var item = await _inventoryRepository.GetByInstanceIdAsync(request.ItemInstanceId);
        if (item == null || item.CharacterId != request.CharacterId)
        {
            return new AuctionResult { Success = false, Message = "Item not found in character inventory." };
        }

        if (item.IsEquipped)
        {
            return new AuctionResult { Success = false, Message = "Equipped items cannot be listed in the Auction House." };
        }

        var listing = new AuctionListing
        {
            SellerCharacterId = request.CharacterId,
            ItemInstanceId = request.ItemInstanceId,
            PriceGold = request.PriceGold,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(Math.Clamp(request.DurationHours, 1, 72))
        };

        var listingId = await _auctionRepository.CreateListingAsync(listing);
        listing.Id = listingId;

        return new AuctionResult
        {
            Success = true,
            Message = "Item successfully listed in Auction House.",
            Listing = listing
        };
    }

    public async Task<AuctionResult> BuyItemAsync(BuyAuctionRequest request)
    {
        var session = await _sessionService.GetSessionAsync(request.SessionToken);
        if (session == null || session.ActiveCharacterId != request.BuyerCharacterId)
        {
            return new AuctionResult { Success = false, Message = "Unauthorized session." };
        }

        var listing = await _auctionRepository.GetByIdAsync(request.ListingId);
        if (listing == null || listing.Status != "ACTIVE")
        {
            return new AuctionResult { Success = false, Message = "Listing not found or no longer active." };
        }

        if (listing.SellerCharacterId == request.BuyerCharacterId)
        {
            return new AuctionResult { Success = false, Message = "You cannot buy your own auction listing." };
        }

        // Execute ACID Atomic Purchase Transaction
        var purchased = await _auctionRepository.ExecuteAtomicPurchaseTransactionAsync(
            listing.Id, 
            request.BuyerCharacterId, 
            listing.PriceGold);

        if (!purchased)
        {
            return new AuctionResult { Success = false, Message = "Purchase failed. Insufficient gold or item no longer available." };
        }

        listing.Status = "SOLD";
        return new AuctionResult
        {
            Success = true,
            Message = $"Successfully purchased item for {listing.PriceGold} gold.",
            Listing = listing
        };
    }

    public async Task<AuctionResult> GetActiveListingsAsync()
    {
        var listings = await _auctionRepository.GetActiveListingsAsync();
        return new AuctionResult
        {
            Success = true,
            Message = "Active listings retrieved.",
            Listings = listings
        };
    }

    public async Task<AuctionResult> CancelListingAsync(string sessionToken, Guid sellerCharacterId, Guid listingId)
    {
        var session = await _sessionService.GetSessionAsync(sessionToken);
        if (session == null || session.ActiveCharacterId != sellerCharacterId)
        {
            return new AuctionResult { Success = false, Message = "Unauthorized session." };
        }

        var cancelled = await _auctionRepository.CancelListingAsync(listingId, sellerCharacterId);
        if (!cancelled)
        {
            return new AuctionResult { Success = false, Message = "Failed to cancel listing." };
        }

        return new AuctionResult { Success = true, Message = "Auction listing cancelled." };
    }
}
