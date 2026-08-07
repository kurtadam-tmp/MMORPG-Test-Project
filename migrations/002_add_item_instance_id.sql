-- 002: Add Unique Instance ID to Inventories for Anti-Dupe Tracking

ALTER TABLE inventories
ADD COLUMN IF NOT EXISTS instance_id UUID DEFAULT gen_random_uuid() NOT NULL;

-- Create unique index on instance_id to prevent duplicate item instances
CREATE UNIQUE INDEX IF NOT EXISTS idx_inventories_instance_id ON inventories(instance_id);
