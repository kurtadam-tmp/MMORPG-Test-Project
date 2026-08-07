using System.Data;
using Dapper;
using MMORPG.Domain.Entities;
using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Data;

namespace MMORPG.Infrastructure.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuctionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AuctionListing?> GetByIdAsync(Guid listingId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, seller_character_id AS SellerCharacterId, item_instance_id AS ItemInstanceId, 
                   price_gold AS PriceGold, status, created_at AS CreatedAt, expires_at AS ExpiresAt
            FROM auction_listings
            WHERE id = @ListingId;";

        return await db.QuerySingleOrDefaultAsync<AuctionListing>(sql, new { ListingId = listingId });
    }

    public async Task<IEnumerable<AuctionListing>> GetActiveListingsAsync()
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, seller_character_id AS SellerCharacterId, item_instance_id AS ItemInstanceId, 
                   price_gold AS PriceGold, status, created_at AS CreatedAt, expires_at AS ExpiresAt
            FROM auction_listings
            WHERE status = 'ACTIVE' AND expires_at > CURRENT_TIMESTAMP
            ORDER BY created_at DESC;";

        return await db.QueryAsync<AuctionListing>(sql);
    }

    public async Task<Guid> CreateListingAsync(AuctionListing listing)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO auction_listings (seller_character_id, item_instance_id, price_gold, status, expires_at)
            VALUES (@SellerCharacterId, @ItemInstanceId, @PriceGold, 'ACTIVE', @ExpiresAt)
            RETURNING id;";

        return await db.ExecuteScalarAsync<Guid>(sql, listing);
    }

    public async Task<bool> ExecuteAtomicPurchaseTransactionAsync(Guid listingId, Guid buyerCharacterId, long priceGold)
    {
        using var db = _connectionFactory.CreateConnection();
        db.Open();
        using var transaction = db.BeginTransaction();

        try
        {
            // 1. Lock and Verify Listing Status
            const string checkListingSql = @"
                SELECT seller_character_id AS SellerCharacterId, item_instance_id AS ItemInstanceId, status
                FROM auction_listings
                WHERE id = @ListingId FOR UPDATE;";

            var listing = await db.QuerySingleOrDefaultAsync<AuctionListing>(checkListingSql, new { ListingId = listingId }, transaction);
            if (listing == null || listing.Status != "ACTIVE")
            {
                transaction.Rollback();
                return false;
            }

            Guid sellerId = listing.SellerCharacterId;
            Guid instanceId = listing.ItemInstanceId;

            // 2. Lock & Deduct Buyer Gold
            const string deductBuyerGoldSql = @"
                UPDATE stats
                SET gold = gold - @PriceGold
                WHERE character_id = @BuyerCharacterId AND gold >= @PriceGold;";

            var buyerDeducted = await db.ExecuteAsync(deductBuyerGoldSql, new { BuyerCharacterId = buyerCharacterId, PriceGold = priceGold }, transaction);
            if (buyerDeducted == 0)
            {
                transaction.Rollback();
                return false;
            }

            // 3. Add Gold to Seller
            const string addSellerGoldSql = @"
                UPDATE stats
                SET gold = gold + @PriceGold
                WHERE character_id = @SellerId;";

            await db.ExecuteAsync(addSellerGoldSql, new { SellerId = sellerId, PriceGold = priceGold }, transaction);

            // 4. Transfer Item to Buyer's Character Inventory
            const string transferItemSql = @"
                UPDATE inventories
                SET character_id = @BuyerCharacterId, is_equipped = FALSE, equip_slot = NULL
                WHERE instance_id = @InstanceId;";

            await db.ExecuteAsync(transferItemSql, new { BuyerCharacterId = buyerCharacterId, InstanceId = instanceId }, transaction);

            // 5. Update Listing Status to 'SOLD'
            const string updateListingSql = @"
                UPDATE auction_listings
                SET status = 'SOLD'
                WHERE id = @ListingId;";

            await db.ExecuteAsync(updateListingSql, new { ListingId = listingId }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> CancelListingAsync(Guid listingId, Guid sellerCharacterId)
    {
        using var db = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE auction_listings
            SET status = 'CANCELLED'
            WHERE id = @ListingId AND seller_character_id = @SellerCharacterId AND status = 'ACTIVE';";

        var affected = await db.ExecuteAsync(sql, new { ListingId = listingId, SellerCharacterId = sellerCharacterId });
        return affected > 0;
    }
}
