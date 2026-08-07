-- MMORPG Dedicated Server PostgreSQL Production Initializer

CREATE TABLE IF NOT EXISTS players (
    id UUID PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS characters (
    id UUID PRIMARY KEY,
    player_id UUID REFERENCES players(id) ON DELETE CASCADE,
    name VARCHAR(50) UNIQUE NOT NULL,
    character_class VARCHAR(30) NOT NULL,
    level INT DEFAULT 1,
    experience BIGINT DEFAULT 0,
    gold BIGINT DEFAULT 1000,
    current_zone_id INT DEFAULT 1,
    pos_x FLOAT DEFAULT 0.0,
    pos_y FLOAT DEFAULT 0.0,
    pos_z FLOAT DEFAULT 0.0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS inventories (
    id UUID PRIMARY KEY,
    character_id UUID REFERENCES characters(id) ON DELETE CASCADE,
    item_id VARCHAR(50) NOT NULL,
    item_name VARCHAR(100) NOT NULL,
    quantity INT DEFAULT 1,
    is_equipped BOOLEAN DEFAULT FALSE,
    enhancement_level INT DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_characters_player_id ON characters(player_id);
CREATE INDEX IF NOT EXISTS idx_inventories_character_id ON inventories(character_id);
