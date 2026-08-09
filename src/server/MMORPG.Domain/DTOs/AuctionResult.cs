using MMORPG.Domain.Entities;

namespace MMORPG.Domain.DTOs;

public class AuctionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AuctionListing? Listing { get; set; }
    public IEnumerable<AuctionListing>? Listings { get; set; }
}
