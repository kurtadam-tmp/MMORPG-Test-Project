-- Enable pgcrypto for UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Timestamp update trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 1. Players Table (Accounts / Users)
CREATE TABLE IF NOT EXISTS players (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    status VARCHAR(20) DEFAULT 'ACTIVE' NOT NULL CHECK (status IN ('ACTIVE', 'BANNED', 'SUSPENDED')),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    last_login_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_players_username ON players(username);
CREATE INDEX IF NOT EXISTS idx_players_email ON players(email);

-- 2. Characters Table
CREATE TABLE IF NOT EXISTS characters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    name VARCHAR(32) UNIQUE NOT NULL,
    level INT DEFAULT 1 NOT NULL CHECK (level >= 1),
    experience BIGINT DEFAULT 0 NOT NULL CHECK (experience >= 0),
    character_class VARCHAR(20) NOT NULL,
    pos_x REAL DEFAULT 0.0 NOT NULL,
    pos_y REAL DEFAULT 0.0 NOT NULL,
    pos_z REAL DEFAULT 0.0 NOT NULL,
    zone_id INT DEFAULT 1 NOT NULL,
    is_deleted BOOLEAN DEFAULT FALSE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_characters_player_id ON characters(player_id);
CREATE INDEX IF NOT EXISTS idx_characters_name ON characters(name);

CREATE TRIGGER trigger_update_characters_updated_at
BEFORE UPDATE ON characters
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- 3. Stats Table (1-to-1 relationship with character)
CREATE TABLE IF NOT EXISTS stats (
    character_id UUID PRIMARY KEY REFERENCES characters(id) ON DELETE CASCADE,
    strength INT DEFAULT 10 NOT NULL CHECK (strength >= 0),
    agility INT DEFAULT 10 NOT NULL CHECK (agility >= 0),
    intelligence INT DEFAULT 10 NOT NULL CHECK (intelligence >= 0),
    vitality INT DEFAULT 10 NOT NULL CHECK (vitality >= 0),
    current_hp INT DEFAULT 100 NOT NULL CHECK (current_hp >= 0),
    max_hp INT DEFAULT 100 NOT NULL CHECK (max_hp > 0),
    current_mp INT DEFAULT 50 NOT NULL CHECK (current_mp >= 0),
    max_mp INT DEFAULT 50 NOT NULL CHECK (max_mp >= 0),
    unallocated_points INT DEFAULT 0 NOT NULL CHECK (unallocated_points >= 0),
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE TRIGGER trigger_update_stats_updated_at
BEFORE UPDATE ON stats
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();

-- 4. Inventories Table (Slot-based inventory management)
CREATE TABLE IF NOT EXISTS inventories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    slot_index INT NOT NULL CHECK (slot_index >= 0 AND slot_index < 100),
    item_id VARCHAR(64) NOT NULL,
    quantity INT DEFAULT 1 NOT NULL CHECK (quantity > 0),
    durability INT DEFAULT 100 NOT NULL CHECK (durability >= 0),
    attributes JSONB DEFAULT '{}'::jsonb NOT NULL,
    is_equipped BOOLEAN DEFAULT FALSE NOT NULL,
    equip_slot VARCHAR(32),
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_character_slot UNIQUE (character_id, slot_index)
);

CREATE INDEX IF NOT EXISTS idx_inventories_character_id ON inventories(character_id);
CREATE INDEX IF NOT EXISTS idx_inventories_equipped ON inventories(character_id, is_equipped) WHERE is_equipped = TRUE;

CREATE TRIGGER trigger_update_inventories_updated_at
BEFORE UPDATE ON inventories
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();
