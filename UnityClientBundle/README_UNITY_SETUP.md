# Unity Client Assets & Export Bundle

This directory contains the pre-compiled **`MMORPG.Shared.dll`** library and complete Unity C# scripts required to connect any Unity project directly to the running MMORPG Dedicated Server cluster.

---

## 📁 Directory Topology

```text
UnityClientBundle/
├── Plugins/
│   └── MMORPG.Shared.dll                  <-- Portable .NET Standard 2.1 Library
└── Scripts/
    ├── Network/
    │   └── MMORPGUnityClientExample.cs    <-- Main UDP Socket Manager & 30 Hz WASD loop
    ├── UI/
    │   ├── LoginUIController.cs           <-- REST API Login/Register Canvas Controller
    │   ├── CharacterSelectUIController.cs <-- REST API Character List & Selection
    │   └── HUDUIController.cs             <-- In-Game Health/Mana Bar & Chat UI
    ├── Visuals/
    │   ├── FloatingHealthBar.cs           <-- Billboard World-Space Health Bar
    │   ├── DamageTextManager.cs           <-- Floating Damage Text Numbers
    │   └── VFXManager.cs                  <-- Skill & Impact Particle Pooler
    └── Sync/
        └── CharacterNetworkSync.cs        <-- Dead-reckoning Lerp position sync
```

---

## 🚀 Quick Setup Instructions

1. **Importing into Unity:**
   - Copy the entire `UnityClientBundle/` folder into your Unity project's `Assets/` directory (e.g. `Assets/MMORPGClient/`).
2. **Dedicated Server Target Settings:**
   - Attach `MMORPGUnityClientExample.cs` to a GameObject in your scene.
   - Set `ServerIp` = `127.0.0.1` and `ServerPort` = `7777` (Main Zone), `7778` (Overflow Zone), or `7779` (Dungeon Zone #99).
3. **Press Play in Unity:**
   - The Unity client will connect UDP socket to the active 30 Hz Dedicated Zone Server!
