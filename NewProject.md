# UAsset MMORPG System Architecture - FINAL SPEC

## Core Design Principles
- **Zero allocations in Update loops**
- **Everything pooled** (objects, transforms, animations, UI)
- **Zone-based world with scene caching**
- **On-demand hybrid item generation**
- **Full in-game editing for server admin**
- **LAN multiplayer (WebSocket, max 6 players)**
- **Target: 40fps on Samsung A15 / 4GB RAM laptop**

## System Hierarchy

```
GameBootstrap (DontDestroyOnLoad)
├── CoreSystemManager
│   ├── ObjectPoolManager
│   ├── ProceduralGenerationCoordinator
│   ├── WebSocketNetworkManager (LAN host/client)
│   ├── SaveSystemManager
│   └── AdminConsoleManager (in-game editor access)
│
├── ZoneSystemManager
│   ├── ZoneSceneManager (load/unload/cache zones as Unity scenes)
│   ├── SimpleTerrainGenerator (per-zone)
│   ├── ZoneBoundaryController (invisible walls)
│   ├── InteractableRegistry (doors, chests, ladders, stairs)
│   └── SpawnPointRegistry (editor-placed empties)
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
│   ├── ItemGenerationEngine (hybrid: pre-gen + on-demand)
│   ├── LootTableManager
│   ├── EquipmentManager
│   └── ConsumableManager
│
├── UISystemManager
│   ├── HUDController (Doom-style)
│   ├── InventoryUIController (D&D-style)
│   ├── CharacterCreationUI (heroic start)
│   ├── QuestTrackerUI
│   ├── AdminEditorUI (weapon/armor/spell/pool editors)
│   └── UIPoolManager
│
└── AudioSystemManager
    ├── SFXPoolManager
    └── MusicController
```

## Zone System Architecture

### Zone Structure
```
Zone = Unity Scene with:
├── Terrain mesh (procedurally generated, cached)
├── Spawn points (enemies, NPCs, player start)
├── Interactables (doors, chests, ladders, stairs, ramps)
├── Boundary colliders (prevent falling off map)
├── Lighting + ambiance
└── Zone metadata (name, level range, biome type)

Zone Types:
- Town (safe, NPCs, shops, quests)
- Dungeon (combat, loot, bosses)
- Wilderness (open exploration, resource gathering)
- Arena (PvP, instanced)
```

### ZoneSceneManager
```csharp
// Helper script to save generated zones as Unity scenes
public class ZoneSceneManager
{
    // Generate zone at runtime, then save as scene
    public async Awaitable GenerateAndSaveZone(ZoneConfig config);
    
    // Load zone scene additively
    public async Awaitable<Scene> LoadZone(string zoneName);
    
    // Unload zone and cache data
    public async Awaitable UnloadZone(string zoneName);
    
    // Check if zone is already cached
    public bool IsZoneCached(string zoneName);
}
```

### Zone Generation Process
1. **Initial generation** (startup or first visit):
   - Generate terrain mesh using Perlin noise
   - Place interactables based on zone config
   - Spawn enemies/NPCs from pools
   - Bake lighting if enabled
   
2. **Save as scene**:
   - Unity Editor: Save generated content as .unity scene file
   - Runtime: Serialize zone state to ScriptableObject
   
3. **Load from cache**:
   - Check if scene exists
   - Load additively with LoadSceneAsync
   - Restore entity states from pool

### Terrain Interactables

```csharp
// All interactables inherit from this
public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(PlayerController player);
    bool CanInteract(PlayerController player);
}

Interactable Types:
├── Door (open/close, locked/unlocked)
├── Chest (loot container)
├── Ladder (climb trigger)
├── Stairs/Ramps (walkable slopes)
├── Window (breakable, climbable)
├── Portal (zone transition)
├── Lever/Button (trigger events)
└── NPC (dialogue, quests, shop)
```

## Character Creation System

### Creation Flow
```
1. Species Selection
   ├── Human (balanced stats)
   ├── Elf (high dex, magic affinity)
   ├── Dwarf (high vitality, strength)
   ├── Orc (high strength, low intelligence)
   ├── Lizardfolk (poison resist, natural armor)
   ├── Android (tech affinity, no magic)
   ├── Cyborg (hybrid tech/bio)
   ├── Alien (unique abilities)
   ├── Demon (dark magic, fire affinity)
   └── Angel (holy magic, light affinity)

2. Gender Selection
   - Male / Female
   - Affects base model mesh

3. Appearance Customization
   - Body type (slim, average, muscular)
   - Skin tone / texture
   - Face shape (5 presets per species)
   - Hair style (10+ options)
   - Accessories (scars, tattoos, tech implants)

4. Species Ability Selection
   - Each species gets 1 unique passive + 1 active ability
   - Examples:
     * Human: "Adaptable" (+10% XP), "Second Wind" (heal 25% HP, 5min cooldown)
     * Elf: "Keen Senses" (+vision range), "Nature's Step" (turn invisible in forests)
     * Android: "Overcharge" (+tech damage), "System Reboot" (cleanse debuffs)

5. Attribute Distribution
   - Starting pool: 20 points
   - Min per stat: 5, Max per stat: 15
   - Distribute across: STR, DEX, INT, VIT, END, LUCK

6. Heroic Start Bonus
   - Choose 1:
     * Legendary Weapon (rare starting weapon)
     * Ancient Armor (rare starting armor set)
     * Spellbook (3 starting spells)
     * Companion (pet/summon)
     * Gold Hoard (5000 starting gold)
```

### Procedural Model Generation
```csharp
public class ProceduralCharacterBuilder
{
    // Generate based on creation choices
    public GameObject GenerateCharacter(CharacterCreationData data)
    {
        // Combine base meshes
        Mesh body = GetSpeciesBodyMesh(data.species, data.gender, data.bodyType);
        Mesh head = GetHeadMesh(data.species, data.faceShape);
        Mesh hair = GetHairMesh(data.hairStyle);
        
        // Apply textures
        Material skin = GenerateSkinMaterial(data.skinTone, data.species);
        
        // Combine into single mesh (optimized)
        Mesh finalMesh = CombineMeshes(body, head, hair);
        
        // Setup rig for animation
        return SetupCharacterRig(finalMesh, skin);
    }
}
```

## Item Generation System (Hybrid Approach)

### Pre-Generation Phase (Startup)
```
Common Archetypes (cached at boot):
- 10 weapon types × 5 tiers = 50 weapons
- 8 armor pieces × 5 tiers = 40 armor
- 15 consumable types = 15 items
Total: ~100 pre-generated items in pool
```

### On-Demand Generation (During Gameplay)
```
When loot drops:
1. Roll rarity tier
2. Check if matching item exists in pool
3. If not, generate new item procedurally
4. Add to pool for future reuse
5. Cap pool growth at 500 unique items

This balances memory (pre-gen) with variety (on-demand)
```

### Item Archetype Database
```
Weapons (50+ archetypes):
├── Primitive: Stick, Rock, Bone Club, Stone Axe
├── Medieval: Sword, Axe, Hammer, Spear, Bow, Crossbow
├── Modern: Pistol, Rifle, Shotgun, Sniper, Grenade Launcher
├── Sci-Fi: Laser Rifle, Plasma Caster, Railgun, Energy Sword
├── Exotic: Antimatter Cannon, Quantum Blade, Void Siphon
└── Magic: Staff, Wand, Tome, Crystal, Orb, Psionic Amplifier

Each archetype has:
- Base stats (damage, range, speed)
- Mesh generation rules
- Affix pools (what modifiers can roll)
- Visual effects (trails, glows, particles)
```

## Admin Console & In-Game Editors

### Admin UI System (Server Host/Admin Only)
```
Admin Console (F12 to open):
├── Item Database Browser
│   ├── Search/filter items
│   ├── Inspect item stats
│   ├── Edit existing items
│   └── Create new items
│
├── Weapon Editor
│   ├── Mesh preview (real-time)
│   ├── Stat sliders (damage, speed, range)
│   ├── Affix editor (add/remove modifiers)
│   ├── Visual effect editor (trails, glows)
│   └── Save to pool / Spawn in world
│
├── Armor Editor
│   ├── Mesh preview (on character model)
│   ├── Stat editor (armor, resistances)
│   ├── Ability editor (bake abilities into armor)
│   └── Appearance customization
│
├── Spell Editor
│   ├── Spell name/description
│   ├── Damage type/amount
│   ├── AOE radius/shape
│   ├── Cooldown/mana cost
│   ├── Visual effect selection
│   └── Add to spellbook / Grant to player
│
├── Pool Database Navigator
│   ├── View all active pools
│   ├── Inspect pooled objects
│   ├── Force clear/refill pools
│   ├── View pool statistics (hits/misses)
│   └── Performance metrics
│
├── Entity Inspector
│   ├── List all entities in zone
│   ├── Select entity to inspect
│   ├── Edit stats (HP, damage, level)
│   ├── Change AI behavior
│   └── Teleport entity
│
├── Zone Editor
│   ├── Regenerate terrain
│   ├── Place/remove interactables
│   ├── Edit spawn points
│   ├── Adjust lighting/ambiance
│   └── Save zone changes
│
└── Player Management
    ├── View all connected players
    ├── Grant items/gold/XP
    ├── Teleport players
    ├── Kick/ban players
    └── Edit player stats
```

### Object Inspection System
```csharp
// Right-click any object in world (admin only)
public class AdminObjectInspector
{
    public void InspectObject(GameObject target)
    {
        // Open inspector UI with:
        // - Object name/type
        // - Transform data (editable)
        // - Component list (editable)
        // - Stats/properties (editable)
        // - Actions (delete, clone, save to pool)
    }
}
```

### Real-Time Editing
- All changes apply immediately (no restart)
- Changes are network-synced to clients
- Changes can be saved to zone or item pool
- Undo/redo support for admin actions

## Network Architecture (LAN WebSocket)

### Host/Client Model
```
Host (Server Admin):
- Runs full simulation
- Authoritative on combat/loot
- Can edit everything via admin console
- Broadcasts state to clients

Clients (Players):
- Client-authoritative movement (smooth)
- Send input to server
- Receive world state updates
- Cannot access admin console
```

### WebSocket Message Types
```csharp
// Movement (client → server)
PlayerMovementUpdate { position, rotation, velocity }

// Combat (server → clients)
DamageEvent { targetId, damage, damageType, isCrit }

// Loot (server → client)
LootDropNotification { itemId, position, rarity }

// Admin Actions (server → clients)
AdminCommand { type, targetId, data }

// Entity Spawn (server → clients)
EntitySpawnEvent { entityId, prefabId, position, stats }
```

### Performance Optimizations for 6 Players
- Update rate: 20Hz (every 50ms)
- Interpolation on clients for smooth movement
- Only sync visible entities (culling)
- Compress messages with MessagePack
- Delta compression for stat updates

## Combat System (RPG-Tanky but Fun Early)

### Damage Calculation
```csharp
float finalDamage = baseDamage 
    * (1 + (attackerStat / 100f)) // STR/DEX/INT
    * (1 + critMultiplier) // if crit
    * elementalMultiplier // type advantage
    * (1 - (defenderArmor / (defenderArmor + 100f))); // diminishing returns
```

### Early Game Pacing
```
Level 1:
- Player: 200 HP, 50 damage/hit
- Basic Enemy: 150 HP, 20 damage/hit
- Time to kill: 3-4 player hits, 10 enemy hits
- Frequent loot: 70% drop rate, +5-10% stat upgrades

Level 10:
- Player: 800 HP, 200 damage/hit
- Mid Enemy: 600 HP, 80 damage/hit
- TTK scales proportionally

Level 50+:
- Player: 5000+ HP, 2000+ damage/hit
- Boss: 50,000 HP, 500 damage/hit
- Epic fights, big damage numbers
```

### Ability System
```
Ability Sources:
1. Equipment (weapon/armor abilities)
2. Spellbook (learned spells)
3. Consumables (one-time effects)
4. Species abilities (racial passives/actives)

Hotbar: 10 slots (1-9, 0)
- Drag abilities from spellbook
- Cooldowns visible on icons
- Mana/stamina costs displayed

Resource Management:
- Mana: 100 base + (INT × 10)
- Stamina: 100 base + (END × 10)
- Regen: 5%/second out of combat
```

## Loot System

### Drop Rates (by source)
```
Enemy Kill:
- Common: 50%
- Uncommon: 25%
- Rare: 10%
- Epic: 3%
- Legendary: 1%
- Mythic: 0.1%

Chest:
- Always drops 2-5 items
- Rarity +1 tier shift vs enemies

Boss Kill:
- Always drops 5-10 items
- Rarity +2 tier shift
- Guaranteed 1+ epic/legendary
```

### Loot Beam Visualization
```
Each rarity has distinct beam:
- Common: White beam, 5m tall
- Uncommon: Green beam, 7m tall
- Rare: Blue beam, 10m tall, particles
- Epic: Purple beam, 15m tall, glow
- Legendary: Orange beam, 20m tall, sparkles
- Mythic: Red beam, 30m tall, lightning
```

## UI Design

### HUD (Doom-style, minimal)
```
┌─────────────────────────────────────┐
│         Quest: Kill 5 Orcs          │ ← Top center
│                                     │
│                                     │
│                                     │
│            + Crosshair              │ ← Center
│                                     │
│                                     │
│  [Minimap]              [Abilities] │ ← Top corners
│                                     │
│                                     │
│    ████ HP  ████ MP  ████ SP        │ ← Bottom center (large)
│        1234    567    890           │
└─────────────────────────────────────┘
```

### Menus (D&D-style, detailed)
```
Character Sheet:
- Full stat breakdown
- Attribute points remaining
- Equipped gear visualization
- Active buffs/debuffs

Inventory:
- Grid-based (10×10)
- Sort by: Type, Rarity, Recent
- Filter buttons (weapons, armor, consumables)
- Detailed tooltips on hover

Spellbook:
- Tree view of learned spells
- Drag to hotbar
- Upgrade spells with skill points
- Preview spell effects
```

## Technical Implementation Stack

### Core Systems (Build Order)
1. **GameBootstrap** + **CoreSystemManager**
2. **ObjectPoolManager** (generic pooling)
3. **ZoneSceneManager** (scene caching)
4. **SimpleTerrainGenerator** (mesh gen + biomes)
5. **InteractableRegistry** (doors, chests, ladders)
6. **ProceduralCharacterBuilder** (species models)
7. **ItemGenerationEngine** (hybrid weapon/armor/consumables)
8. **CombatSystemManager** (damage calc + abilities)
9. **WebSocketNetworkManager** (LAN multiplayer)
10. **UISystemManager** (HUD + menus + admin console)
11. **AdminEditorUI** (all in-game editors)

### Performance Budget (40fps = 25ms frame time)
```
Update Loop Budget:
- Input: 2ms
- Physics: 5ms
- Rendering: 10ms
- Audio: 1ms
- Network: 2ms
- Systems: 5ms
Total: 25ms
```

### Serialization Pattern
```csharp
// All systems expose runtime references
[Header("Runtime References (Auto-Assigned)")]
[SerializeField] private GameObject _playerInstance;
[SerializeField] private List<SpawnPoint> _spawnPoints;
[SerializeField] private Transform _zoneRoot;

// Visible in inspector before first run
// Can manually link if needed
// Auto-assigned during Awake/Start
```

---

## Next Implementation Steps

I'll now build these systems in order, starting with:
1. GameBootstrap + CoreSystemManager (foundation)
2. ObjectPoolManager (IPoolable + generic pools)
3. ZoneSceneManager (scene loading/saving)
4. SimpleTerrainGenerator (optimized mesh generation)

Each system will be:
- Fully commented
- Zero allocations in Update
- Burst-compatible where applicable
- Serialized references for editor inspection
- Admin-editable via UI

Ready to start building?