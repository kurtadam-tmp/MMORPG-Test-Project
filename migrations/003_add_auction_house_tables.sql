-- 003: Auction House Tables & Gold Columns Setup

-- Create Auction Listings Table
CREATE TABLE IF NOT EXISTS auction_listings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    seller_character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    item_instance_id UUID NOT NULL REFERENCES inventories(instance_id) ON DELETE CASCADE,
    price_gold BIGINT NOT NULL CHECK (price_gold > 0),
    status VARCHAR(20) DEFAULT 'ACTIVE' NOT NULL CHECK (status IN ('ACTIVE', 'SOLD', 'CANCELLED', 'EXPIRED')),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_auction_status ON auction_listings(status);
CREATE INDEX IF NOT EXISTS idx_auction_seller ON auction_listings(seller_character_id);
CREATE INDEX IF NOT EXISTS idx_auction_item ON auction_listings(item_instance_id);

-- Add Gold column to stats table if not existing
ALTER TABLE stats
ADD COLUMN IF NOT EXISTS gold BIGINT DEFAULT 100 NOT NULL CHECK (gold >= 0);
