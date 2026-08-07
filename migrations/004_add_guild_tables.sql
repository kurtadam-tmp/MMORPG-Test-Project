-- 004: Guild & Clan System Tables Setup

CREATE TABLE IF NOT EXISTS guilds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    leader_character_id UUID NOT NULL REFERENCES characters(id) ON DELETE RESTRICT,
    vault_gold BIGINT DEFAULT 0 NOT NULL CHECK (vault_gold >= 0),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS guild_members (
    guild_id UUID NOT NULL REFERENCES guilds(id) ON DELETE CASCADE,
    character_id UUID NOT NULL UNIQUE REFERENCES characters(id) ON DELETE CASCADE,
    rank VARCHAR(20) DEFAULT 'MEMBER' NOT NULL CHECK (rank IN ('LEADER', 'OFFICER', 'MEMBER')),
    joined_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    PRIMARY KEY (guild_id, character_id)
);

CREATE INDEX IF NOT EXISTS idx_guild_name ON guilds(name);
CREATE INDEX IF NOT EXISTS idx_guild_member_char ON guild_members(character_id);
