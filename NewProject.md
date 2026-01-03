# UAsset MMORPG System Architecture

## Core Design Principles
- **Zero allocations in Update loops**
- **Everything pooled** (objects, transforms, animations, UI)
- **Procedural generation at startup/async** (not runtime)
- **Serialized references** for editor inspection before runtime
- **Target: 40fps on Samsung A15 / 4GB RAM laptop**

## System Hierarchy

```
GameBootstrap (DontDestroyOnLoad)
├── CoreSystemManager
│   ├── ObjectPoolManager
│   ├── ProceduralGenerationCoordinator
│   ├── NetworkManager (single/multi)
│   └── SaveSystemManager
│
├── WorldSystemManager
│   ├── SimpleTerrainGenerator (rolling hills, biomes)
│   ├── SpawnPointRegistry (editor-placed empties)
│   └── WorldStreamingController
│
├── EntitySystemManager
│   ├── PlayerManager
│   ├── NPCManager (humanoid/creature variants)
│   ├── EnemyManager
│   └── EntityPoolCoordinator
│
├── CombatSystemManager
│   ├── DamageCalculator
│   ├── ProjectilePoolManager
│   ├── AbilitySystemController
│   └── MagicSystemController
│
├── InventorySystemManager
│   ├── ItemGenerationEngine
│   ├── LootTableManager
│   ├── EquipmentManager
│   └── ConsumableManager
│
├── UISystemManager
│   ├── HUDController (Doom-style)
│   ├── InventoryUIController (D&D-style)
│   ├── QuestTrackerUI
│   └── UIPoolManager
│
└── AudioSystemManager
    ├── SFXPoolManager
    └── MusicController
```

## Object Pooling Strategy

### Pool Categories
1. **Combat Pools** (pre-warmed at scene load)
   - Projectiles (bullets, arrows, magic missiles): 500 instances
   - Damage numbers: 100 instances
   - Hit effects (sparks, blood, magic): 200 instances
   - AOE effects: 50 instances

2. **Entity Pools** (lazy load with growth caps)
   - Enemies: 50-200 based on zone
   - NPCs: 20-50 based on area
   - Loot drops: 100 instances

3. **UI Pools** (pre-warmed)
   - Inventory slots: 200 instances
   - Tooltip windows: 10 instances
   - Quest notifications: 20 instances
   - Damage/XP popups: 50 instances

4. **Audio Pools**
   - Audio sources: 32 instances (spatial)
   - UI audio sources: 8 instances (2D)

### Pool Manager API
```csharp
// Get from pool (zero allocation)
T Get<T>() where T : IPoolable;

// Return to pool
void Return<T>(T obj) where T : IPoolable;

// Pre-warm specific pool
void WarmPool<T>(int count) where T : IPoolable;
```

## Procedural Generation Layers

### 1. World Generation (Startup)
- **Simple terrain**: Rolling hills, flat plateaus, shallow valleys
- **Biomes**: 5 types (grassland, desert, snow, lava, corrupted)
- **Generation method**: Perlin noise + biome masks
- **Chunk size**: 100x100 units, pooled mesh generation

### 2. Item Generation (On-demand + Cached)
```
Item Archetype Categories:
├── Weapons (50+ archetypes)
│   ├── Melee: Stick, Sword, Hammer, Axe, Spear, Energy Blade, Plasma Whip
│   ├── Ranged: Bow, Crossbow, Pistol, Rifle, Shotgun, Railgun, Plasma Caster
│   └── Magic: Staff, Wand, Tome, Crystal, Psionic Amp, Void Channeler
│
├── Armor (30+ archetypes)
│   ├── Head: Cloth Hood, Leather Cap, Iron Helm, Power Helmet, Psionic Crown
│   ├── Body: Rags, Leather Vest, Plate Armor, Exosuit, Energy Shield
│   ├── Hands: Wraps, Gloves, Gauntlets, Cybernetic Arms, Phase Shifters
│   └── Feet: Sandals, Boots, Greaves, Hover Boots, Quantum Treads
│
├── Consumables
│   ├── Healing: Bandages, Potions, Med-kits, Nano-bots
│   ├── Buffs: Strength Tonic, Speed Serum, Shield Boost, Berserk Pill
│   └── Combat: Grenades, Traps, Scrolls, AOE Devices
│
└── Materials (crafting/upgrade)
    ├── Common: Wood, Stone, Iron, Cloth
    ├── Rare: Mithril, Obsidian, Ether Crystal
    └── Legendary: Antimatter Core, Quantum Filament, Void Essence
```

### 3. Character Model Generation
```
Species Templates (Male/Female variants):
- Human, Elf, Dwarf, Orc, Lizardfolk
- Android, Cyborg, Alien, Demon, Angel
- Undead, Construct, Elemental, Hybrid

Body Part System:
├── Head (5 base shapes per species)
├── Torso (3 builds: slim, average, muscular)
├── Arms (proportional to torso)
├── Legs (proportional to torso)
└── Texture variants (skin tones, patterns, tech)

Generation: Combine base meshes + procedural textures
```

### 4. Animation Transform Pooling
```
AnimationStatePool:
- Idle variations (5)
- Walk/Run cycles (3 speeds)
- Combat stances (weapon-specific)
- Ability casting (magic types)
- Hit reactions (8 directions)
- Death animations (5 variants)

Pool Management:
- Pre-bake transform keyframes
- Store as NativeArray<Matrix4x4>
- Blend between pools using indices
- Zero allocation during playback
```

## Combat System Design

### Stat System (RPG-Tanky but Fun Early)
```
Primary Stats:
- Strength (melee damage, carry weight)
- Dexterity (ranged damage, dodge, attack speed)
- Intelligence (magic power, mana pool)
- Vitality (health, health regen)
- Endurance (stamina, stamina regen)
- Luck (crit chance, loot quality)

Derived Stats:
- Armor (damage reduction %)
- Resistances (elemental, magic, physical)
- Damage multipliers (type-specific)
- Movement speed modifiers
```

### Early Game Balance
- Level 1: ~200 HP, 50 damage/hit, 4-5 hits to kill basic enemy
- Enemy density: Low initially, scales with player power
- Loot drops: Frequent small upgrades (+5-10% stats)
- Level scaling: Exponential HP/damage growth

### Ability & Magic System
```
Ability Sources:
1. Equipment-based (items grant abilities)
2. Consumable-triggered (scrolls, devices)
3. Class skills (unlocked via leveling)

Ability Types:
├── Direct Damage (instant cast)
├── AOE (pooled particle effects)
├── Buffs/Debuffs (timed modifiers)
├── Summons (pooled entity)
├── Movement (dash, teleport, flight)
└── Utility (stealth, shield, heal)

Resource System:
- Mana (magic abilities)
- Stamina (physical abilities)
- Cooldowns (per-ability timers)
- Item charges (equipment abilities)
```

## Loot System Philosophy

### Loot Everywhere Approach
- **Enemy kills**: 80% drop rate, 1-3 items
- **Containers**: Chests, barrels, crates (100% drop)
- **Gathering**: Resources from environment
- **Quest rewards**: Guaranteed meaningful drops
- **Boss kills**: 5-10 items, higher rarity

### Rarity Tiers
- Common (white): 60% drop rate
- Uncommon (green): 25% drop rate
- Rare (blue): 10% drop rate
- Epic (purple): 4% drop rate
- Legendary (orange): 0.9% drop rate
- Mythic (red): 0.1% drop rate

### Item Affixes (Diablo-style)
- Prefix: +Damage, +Elemental, +Critical
- Suffix: +Health, +Resistance, +Ability
- Number of affixes scales with rarity
- Procedurally generated stat ranges

## UI Design (D&D + Doom Blend)

### HUD (Doom-style)
- Bottom center: Health/Mana/Stamina bars (large, visible)
- Bottom corners: Ammo/ability cooldowns
- Top right: Mini-map, quest tracker
- Center: Crosshair with hit markers
- Minimal chrome, maximum visibility

### Menus (D&D-style)
- Character sheet: Full stat breakdown
- Inventory: Grid-based with filters
- Equipment: Paper-doll with slots
- Abilities: Skill tree visualization
- Quest log: Nested objectives

### Visual Theme
- Neon accents on dark UI (cyberpunk meets fantasy)
- Satirical item descriptions
- Smooth transitions (pooled tweening)
- Bright, colorful loot beams

## Technical Implementation Notes

### Spawn Point System
```csharp
// Editor-placed empty GameObjects with this component
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private SpawnType _type;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private int _maxEntities = 10;
    
    // Auto-registered on Awake
    // Spawner references this at runtime
}
```

### Reference Serialization Pattern
```csharp
public class SystemManager : MonoBehaviour
{
    [Header("Runtime References (Auto-Assigned)")]
    [SerializeField] private GameObject _playerInstance;
    [SerializeField] private Transform _worldRoot;
    [SerializeField] private Camera _mainCamera;
    
    // Visible in inspector, assigned at runtime
    // Can be manually linked before first run if needed
}
```

### Performance Optimizations
1. **LOD for entities**: 3 levels based on distance
2. **Occlusion culling**: Baked per-zone
3. **Texture atlasing**: All items share 2-4 atlases
4. **Mesh batching**: Procedural meshes combined where possible
5. **Audio distance culling**: Stop sounds beyond 50 units
6. **UI pooling**: Reuse slot instances, update data only

## Network Consideration (MMORPG-style)

### Single-player mode
- All systems run locally
- No network overhead

### Multiplayer mode
- Client-authoritative movement (CoD-style responsiveness)
- Server-authoritative combat (prevent cheating)
- Loot instanced per player (Destiny-style)
- Async world streaming

---

## Next Steps
1. Build CoreSystemManager + ObjectPoolManager
2. Build SimpleTerrainGenerator + SpawnPointRegistry
3. Build ItemGenerationEngine with archetype system
4. Build Character/NPC procedural generation
5. Build Combat + Ability system
6. Build UI controllers with pooling
7. Integrate everything with Game Bootstrap

This architecture respects Unity 6.1 optimization requirements while delivering the MMORPG action-RPG hybrid experience you want.
```