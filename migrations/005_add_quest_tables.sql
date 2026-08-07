-- 005: Quest & Reward System Tables Setup

CREATE TABLE IF NOT EXISTS character_quests (
    character_id UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
    quest_id VARCHAR(50) NOT NULL,
    current_progress INT DEFAULT 0 NOT NULL CHECK (current_progress >= 0),
    target_amount INT NOT NULL CHECK (target_amount > 0),
    status VARCHAR(20) DEFAULT 'IN_PROGRESS' NOT NULL CHECK (status IN ('IN_PROGRESS', 'COMPLETED', 'REWARDED')),
    accepted_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    PRIMARY KEY (character_id, quest_id)
);

CREATE INDEX IF NOT EXISTS idx_char_quest_status ON character_quests(character_id, status);
