-- Migration 006: Add New Maps, World Bosses, Mobs and Class Skill Definitions

-- Create maps definition table if not exists
CREATE TABLE IF NOT EXISTS zone_maps (
    id INT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    recommended_level INT NOT NULL,
    max_capacity INT NOT NULL,
    is_dungeon BOOLEAN DEFAULT FALSE
);

INSERT INTO zone_maps (id, name, recommended_level, max_capacity, is_dungeon)
VALUES 
    (1, 'Elwynn Grasslands (Main World)', 1, 500, FALSE),
    (2, 'Ironforge Snow Mountains', 10, 500, FALSE),
    (3, 'Volcano Raid Arena (Ignis Boss)', 40, 200, FALSE),
    (99, 'Dragon''s Lair Dungeon', 50, 50, TRUE)
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Create mob and boss templates table
CREATE TABLE IF NOT EXISTS mob_templates (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    level INT NOT NULL,
    max_hp INT NOT NULL,
    base_damage INT NOT NULL,
    is_boss BOOLEAN DEFAULT FALSE,
    respawn_seconds INT NOT NULL
);

INSERT INTO mob_templates (name, level, max_hp, base_damage, is_boss, respawn_seconds)
VALUES 
    ('Forest Goblin', 1, 150, 15, FALSE, 10),
    ('Wild Wolf', 3, 300, 25, FALSE, 15),
    ('Skeleton Warrior', 5, 550, 45, FALSE, 20),
    ('Lava Elemental', 15, 1800, 120, FALSE, 30),
    ('Inferno Dragon Ignis', 50, 100000, 850, TRUE, 300),
    ('Frost Lich Kel''Thuzis', 60, 250000, 1400, TRUE, 600)
ON CONFLICT DO NOTHING;
