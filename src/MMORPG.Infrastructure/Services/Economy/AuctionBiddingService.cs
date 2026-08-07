using System.Collections.Concurrent;
using MMORPG.Domain.Interfaces;

namespace MMORPG.Infrastructure.Services;

public class AuctionBiddingService : IAuctionBiddingService
{
    private readonly ConcurrentDictionary<Guid, AuctionBidItem> _listings = new();

    public AuctionBidItem CreateListing(Guid sellerId, string itemName, long startingBid, long buyout, int durationHours)
    {
        var item = new AuctionBidItem
        {
            ListingId = Guid.NewGuid(),
            SellerCharacterId = sellerId,
            ItemName = itemName,
            StartingBidGold = startingBid,
            CurrentBidGold = startingBid,
            CurrentHighestBidderId = Guid.Empty,
            BuyoutGold = buyout,
            ExpirationTime = DateTime.UtcNow.AddHours(durationHours),
            IsSold = false,
            IsExpired = false
        };

        _listings[item.ListingId] = item;
        Console.WriteLine($"[AuctionHouse LISTING] Character '{sellerId}' listed '{itemName}' (Starting Bid: {startingBid}g, Buyout: {buyout}g, Duration: {durationHours}h).");
        return item;
    }

    public bool PlaceBid(Guid bidderId, Guid listingId, long bidAmount, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (!_listings.TryGetValue(listingId, out var item))
        {
            resultMessage = "Müzayede ilanı bulunamadı.";
            return false;
        }

        lock (item)
        {
            if (item.IsSold || item.IsExpired || DateTime.UtcNow > item.ExpirationTime)
            {
                resultMessage = "Müzayede ilanı süresi dolmuş veya satılmış.";
                return false;
            }

            long minRequiredBid = item.CurrentHighestBidderId == Guid.Empty ? item.StartingBidGold : (long)(item.CurrentBidGold * 1.05);
            if (bidAmount < minRequiredBid)
            {
                resultMessage = $"Minimum teklif tutarı en az {minRequiredBid} Gold olmalıdır.";
                return false;
            }

            // Refund previous bidder if exists
            if (item.CurrentHighestBidderId != Guid.Empty)
            {
                Console.WriteLine($"[AuctionHouse REFUND] Outbid! Refunded {item.CurrentBidGold} Gold to previous bidder '{item.CurrentHighestBidderId}'.");
            }

            item.CurrentBidGold = bidAmount;
            item.CurrentHighestBidderId = bidderId;

            // If bid reaches or exceeds buyout price, auto buyout!
            if (bidAmount >= item.BuyoutGold && item.BuyoutGold > 0)
            {
                item.IsSold = true;
                resultMessage = $"Teklif Buyout fiyatını karşıladı! '{item.ItemName}' ürünü satın alındı!";
                Console.WriteLine($"[AuctionHouse BUYOUT] Character '{bidderId}' bought out '{item.ItemName}' for {item.CurrentBidGold} Gold!");
                return true;
            }

            resultMessage = $"Teklif başarıyla verildi! Yeni En Yüksek Teklif: {bidAmount} Gold.";
            Console.WriteLine($"[AuctionHouse BID SUCCESS] Character '{bidderId}' placed bid of {bidAmount} Gold on '{item.ItemName}'.");
            return true;
        }
    }

    public bool BuyoutListing(Guid buyerId, Guid listingId, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (!_listings.TryGetValue(listingId, out var item))
        {
            resultMessage = "Müzayede ilanı bulunamadı.";
            return false;
        }

        lock (item)
        {
            if (item.IsSold || item.IsExpired)
            {
                resultMessage = "Müzayede halihazırda sonlanmış.";
                return false;
            }

            item.IsSold = true;
            item.CurrentBidGold = item.BuyoutGold;
            item.CurrentHighestBidderId = buyerId;
            resultMessage = $"Hemen Al işlemi başarılı! '{item.ItemName}' ürünü {item.BuyoutGold} Gold karşılığında satın alındı.";
            Console.WriteLine($"[AuctionHouse BUYOUT SUCCESS] Character '{buyerId}' bought out '{item.ItemName}' for {item.BuyoutGold} Gold.");
            return true;
        }
    }

    public List<AuctionBidItem> ProcessExpiredListings()
    {
        var expiredListings = new List<AuctionBidItem>();
        DateTime now = DateTime.UtcNow;

        foreach (var item in _listings.Values)
        {
            lock (item)
            {
                if (!item.IsSold && !item.IsExpired && now >= item.ExpirationTime)
                {
                    item.IsExpired = true;
                    expiredListings.Add(item);
                    if (item.CurrentHighestBidderId != Guid.Empty)
                    {
                        item.IsSold = true;
                        Console.WriteLine($"[AuctionHouse EXPIRED & SOLD] Listing '{item.ListingId}' expired and sold to highest bidder '{item.CurrentHighestBidderId}' for {item.CurrentBidGold} Gold.");
                    }
                    else
                    {
                        Console.WriteLine($"[AuctionHouse EXPIRED & RETURNED] Listing '{item.ListingId}' expired unsold. Item returned to seller '{item.SellerCharacterId}'.");
                    }
                }
            }
        }

        return expiredListings;
    }
}
