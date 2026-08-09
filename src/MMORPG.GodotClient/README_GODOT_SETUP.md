# MMORPG Godot 4.3 .NET Client Project

This directory contains the complete **Godot 4.3 .NET C#** client implementation for the MMORPG Dedicated Server Cluster.

---

## 📁 Directory Topology

```text
src/MMORPG.GodotClient/
├── MMORPG.GodotClient.csproj   <-- Godot .NET 8.0/10.0 C# Project File
├── project.godot               <-- Main Godot Engine Config
├── Scenes/
│   └── MainWorld.tscn          <-- 2.5D Main Scene
└── Scripts/
    ├── Network/
    │   └── MMORPGGodotClient.cs <-- 30 Hz UDP Socket Network Manager
    ├── UI/
    │   ├── MMORPGMasterGodotUI.cs <-- Master UI (Hotkeys: I, C, E, M, ESC)
    │   └── MMORPGSceneAutoInitializer.cs <-- Scene Auto-Generator
    └── Visuals/
        ├── GodotPlayerVisualizer.cs <-- 2.5D Isometric 45-deg Player Controller
        ├── GodotBossVisualizer.cs   <-- World Boss Ignis (Phase 1-2-3 Telegraph)
        └── GodotZonePortalVisualizer.cs <-- Glowing 2.5D Zone Portal Archway
```

---

## 🚀 Quick Setup Instructions

1. **Launch Godot Engine 4.3 (.NET Mono Edition)**:
   - Click **Import Project**.
   - Select `src/MMORPG.GodotClient/project.godot`.

2. **Press Play (F5)**:
   - The scene `MainWorld.tscn` will automatically boot with 2.5D Isometric 45° Camera follow, World Boss, Zone Portal, UDP Socket Client, and Master UI overlay!

3. **Hotkeys**:
   - **WASD / Arrows**: Move Character.
   - **I**: Toggle Inventory.
   - **C**: Toggle Character Stats (STR/AGI/INT/VIT).
   - **E**: Toggle Demirci Örsü (+9 Basma).
   - **M**: Toggle Minimap.
   - **ESC**: Close windows.
