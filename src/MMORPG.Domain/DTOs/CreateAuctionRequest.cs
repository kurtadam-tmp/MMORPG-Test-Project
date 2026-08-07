namespace MMORPG.Domain.DTOs;

public class CreateAuctionRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public Guid ItemInstanceId { get; set; }
    public long PriceGold { get; set; }
    public int DurationHours { get; set; } = 24;
}
