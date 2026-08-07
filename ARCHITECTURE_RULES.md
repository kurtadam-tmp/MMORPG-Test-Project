# MMORPG Project Architecture & Guidelines (Antigravity Agent Rules)

## 1. Project Overview & Vision
- **Genre:** Isometric 2.5D / Low-Poly MMORPG.
- **Reference Games:** Albion Online, Ragnarok Online, Ultima Online, Diablo.
- **Core Focus:** Deep server-authoritative mechanics, item-based/stat-based combat, player-driven economy, and scalable networking. AAA graphics are NOT a priority; performance and systems take precedence.

## 2. Tech Stack & Infrastructure
- **Game Engine (Client):** Unity (C#) - Isometric / Top-Down camera controller.
- **Dedicated Game Server (Zone Server):** C# (.NET Core) Console / Authoritative Server Loop (Tick Rate: 20-30 Hz).
- **Gateway & Cluster Architecture:** Gateway/Login server handles auth and handoffs; Dedicated Zone Servers manage map instances.
- **Networking Model:** UDP / Reliable UDP (Fish-Net or LiteNetLib / Netcode).
- **Database & Caching Layer:** 
  - **PostgreSQL:** Persistent transactional data (Users, Characters, Inventory, Equipment, Auction House).
  - **Redis:** In-memory caching, real-time sessions, active zone state, inter-server Pub/Sub messaging.
- **Environment:** Local orchestration via Docker Compose.

## 3. Core Networking & Game Logic Rules (STRICT)
- **Server Authority (100% Authoritative):** Never trust the Client. The Client only sends inputs (e.g., "Click at Position X,Y", "Cast Skill Z"). The Server validates movement, cooldowns, stats, hit-boxes, and generates results.
- **Networking Strategy (Albion-style):**
  - **Client-Side Prediction:** Used for local player responsiveness.
  - **Snapshot Interpolation:** Used for smoothing other players/NPC movements.
  - **No Aggressive Rewind (No FPS-style lag compensation):** Combat and AoE validation must be evaluated in **Server-Time**. Use Cast Times (Telegraphs) and Server checks to protect defending players from high-ping unfair hits.
  - **Sanity Checks & Rubberbanding:** Minor position discrepancies must be smoothed via `Lerp`; major invalid movements must trigger server rollback.

## 4. Advanced System & Reliability Principles (NEW)
- **Write-Behind Persistence Pattern:** High-frequency spatial updates (positions, temporary HP/MP changes during combat) are written exclusively to **Redis** on every tick. Persistent PostgreSQL checkpoints occur asynchronously (e.g., every 30 seconds, during zone transfers, or upon disconnection) to eliminate database I/O bottlenecks.
- **Gateway & Zone Server Handshake:** Clients authenticate with the Gateway Server first to receive a single-use Zone Handoff Token before connecting to a Dedicated Zone Server.
- **Inter-Server Communication (Pub/Sub):** Cross-zone events, global channels, guild broadcasts, and player transfer handoffs utilize **Redis Pub/Sub**.
- **Unique Instance Item Tracking (Anti-Dupe):** All non-stackable gear and valuable items must possess a globally unique `instance_id` (UUID). Item transfers must log transaction histories to guarantee 100% economy integrity.
- **Packet Security & Sequence Validation:** Client packet payloads must contain monotonically increasing **Sequence IDs** and timestamp deltas to prevent replay attacks and packet injection.

## 5. Coding Standards & Guidelines for Gemini
- Write modular, clean, clean-architecture compliant C# code.
- Avoid tight coupling between Unity Engine components and Core Domain Logic (keep pure C# logic portable for the Dedicated Server).
- Database operations must use atomic transactions (`BEGIN TRANSACTION ... COMMIT`) for trades, items, and currency updates to guarantee ACID compliance.