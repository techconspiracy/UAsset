# System Architecture

## Overview

UAsset is a procedural RPG game system where characters, items, and equipment are generated entirely at runtime. The core innovation is the **Procedural Character Rig System** where each bone is a capsule with collider and mesh renderer, eliminating the need for imported models or Unity's Animator.

## Component Hierarchy

```
Scene Root
├── GameManager
│   ├── makeItems (Item Generator & Character Spawner)
│   ├── ProceduralWeaponModels
│   ├── ProceduralArmorModels
│   ├── ProceduralIconGenerator
│   └── ProjectileSystem
│
├── Player (Generated at Runtime)
│   ├── CharacterController (Unity)
│   ├── ProceduralCharacterRig ⭐ NEW
│   │   └── 22 Bone GameObjects (each with CapsuleCollider + Mesh)
│   ├── ProceduralAnimationController ⭐ NEW
│   ├── PlayerStats
│   ├── ExperienceManager
│   ├── InventoryManager
│   ├── ThirdPersonController
│   ├── CombatAnimationController
│   ├── PlayerWeaponHandler
│   └── RangedWeaponHandler
│
├── Enemies (Generated at Runtime)
│   ├── CharacterController (Unity)
│   ├── ProceduralCharacterRig ⭐ NEW
│   │   └── 22 Bone GameObjects (each with CapsuleCollider + Mesh)
│   ├── ProceduralAnimationController ⭐ NEW
│   ├── EnemyStats
│   └── EnemyWeaponHandler
│
└── UI
    ├── Canvas
    ├── InventoryPanel (InventoryUI)
    ├── PlayerStatsUI
    └── ExperienceUI
```

## Core Systems

### 1. Procedural Character Rig System ⭐ NEW

**Purpose**: Generate complete character skeletons at runtime where each bone is a physical capsule.

**Key Components**:
- `ProceduralCharacterRig.cs`: Bone generation and hierarchy management
- `ProceduralAnimationController.cs`: Pure code-based animation via Transform rotation

**Bone Structure (22 bones)**:
```
Root → Pelvis → Spine → Chest
                  └─ Neck → Head
                  └─ Left Shoulder → Upper Arm → Lower Arm → Hand
                  └─ Right Shoulder → Upper Arm → Lower Arm → Hand (weapon)
        └─ Left Hip → Upper Leg → Lower Leg → Foot
        └─ Right Hip → Upper Leg → Lower Leg → Foot
```

**Each Bone Includes**:
- GameObject with Transform hierarchy
- CapsuleCollider (physics collision)
- MeshRenderer (visual capsule mesh)
- Rigidbody (kinematic, for stability)
- Configurable dimensions (radius, height)

**Benefits**:
- No imported models needed
- Full physics collision per bone
- Dynamic color/scale customization
- Easy equipment attachment
- ~50KB per character vs MBs for traditional models

### 2. Animation System ⭐ NEW

**Purpose**: Animate characters via code instead of Unity's Animator.

**Animation States**:
- **Idle**: Breathing (chest scale), head turning, subtle sway
- **Walk**: Leg/arm swing (opposite), vertical bobbing, forward lean, knee bending
- **Attack**: Weapon-specific (swing, stab, cast, draw)

**How It Works**:
```csharp
// Example: Walk animation
float legSwing = Mathf.Sin(walkCycle) * 40f;
rightUpperLeg.localRotation = Quaternion.Euler(legSwing, 0, 0);
leftUpperLeg.localRotation = Quaternion.Euler(-legSwing, 0, 0);

// Arms swing opposite
float armSwing = Mathf.Sin(walkCycle) * 30f;
rightUpperArm.localRotation = Quaternion.Euler(-armSwing, 0, 90);
leftUpperArm.localRotation = Quaternion.Euler(armSwing, 0, -90);
```

**Advantages**:
- No Animator state machine overhead
- Fully customizable in code
- Easy debugging and modification
- Performant (direct Transform updates)

### 3. Character Spawning System ⭐ REFACTORED

**Purpose**: Generate complete player and enemy characters on demand.

**Flow**:
```
GameManager.StartGame()
  └─ makeItems.SpawnPlayer(position)
      ├─ Create GameObject "Player"
      ├─ Add CharacterController
      ├─ Add ProceduralCharacterRig
      │   └─ GenerateCompleteRig()
      │       └─ CreateBone() × 22
      │           ├─ CapsuleCollider
      │           ├─ MeshRenderer
      │           └─ Rigidbody
      ├─ SetSkinColor(playerColor)
      ├─ Add ProceduralAnimationController
      ├─ Add PlayerStats
      ├─ Add Combat Systems
      ├─ Add InventoryManager
      └─ Add ThirdPersonController

GameManager.SpawnEnemy(position, level)
  └─ makeItems.SpawnEnemy(position, level)
      ├─ Create GameObject "Enemy_LvlX"
      ├─ Add CharacterController
      ├─ Add ProceduralCharacterRig
      │   └─ GenerateCompleteRig()
      ├─ SetSkinColor(enemyColor + random variation)
      ├─ Add ProceduralAnimationController
      ├─ Add EnemyStats
      ├─ Generate weapon → Attach to hand bone
      └─ Create health bar UI
```

### 4. Item Generation System

**Purpose**: Create weapons, armor, and items with stats and models.

**Flow**:
```
makeItems.GenerateWeapon(level)
  ├─ Determine rarity (Common → Legendary)
  ├─ Generate stats (damage, crit, speed)
  ├─ Create procedural name
  ├─ ProceduralWeaponModels.GenerateWeaponModel()
  │   └─ Create 3D mesh (blade, handle, guard)
  └─ ProceduralIconGenerator.GenerateIcon()
      └─ Return Weapon with model & stats
```

**Rarity System**:
- Common: 50% (base stats)
- Uncommon: 25% (1.3× stats)
- Rare: 15% (1.6× stats)
- Epic: 7% (2× stats)
- Legendary: 3% (2.5× stats)

### 5. Combat System

**Melee Combat Flow**:
```
Player Input (Left Click)
  └─ ThirdPersonController.Attack()
      └─ ProceduralAnimationController.PlayAttack(weaponType)
          ├─ Animate bone rotations (wind up → swing → follow through)
          └─ OnAttackHitFrame()
              └─ PlayerWeaponHandler.DealDamage()
                  └─ EnemyStats.TakeDamage(damage, isCrit)
                      ├─ Apply armor reduction
                      ├─ Show damage text
                      └─ If health <= 0: Die()
                          ├─ Award XP
                          └─ Drop loot
```

**Ranged Combat Flow**:
```
Player Input (Left Click with Bow)
  └─ RangedWeaponHandler.TryRangedAttack()
      ├─ ProceduralAnimationController.PlayAttack(Bow)
      │   └─ Animate draw → hold → release
      └─ ProjectileSystem.FireProjectile()
          └─ Projectile.OnTriggerEnter(enemy)
              └─ EnemyStats.TakeDamage()
```

### 6. Equipment System

**Equipment Attachment Flow**:
```
InventoryUI.OnSlotClicked(item)
  └─ InventoryManager.EquipItem(item)
      └─ PlayerStats.EquipWeapon(weapon) OR EquipArmor(armor)
          └─ Get attachment bone from ProceduralCharacterRig
              ├─ Weapon → GetWeaponBone() = RightHand
              ├─ Helmet → GetArmorBone(Helmet) = Head
              ├─ Chestplate → GetArmorBone(Chestplate) = Chest
              └─ etc...
          └─ item.model.transform.SetParent(bone)
              └─ Update combat systems with new stats
```

**Armor Bone Mapping**:
- Helmet → Head
- Chestplate → Chest
- Leggings → Pelvis
- Gloves → RightHand (or both)
- Boots → RightFoot (or both)
- Shield → LeftLowerArm

### 7. Experience & Leveling System

**XP Flow**:
```
EnemyStats.Die()
  └─ ExperienceManager.GainExperience(amount)
      ├─ currentXP += amount
      ├─ Check if currentXP >= xpToNextLevel
      └─ If leveled up:
          └─ PlayerStats.OnLevelUp(newLevel)
              ├─ Increase maxHealth, maxMana
              ├─ Increase damage, armor
              ├─ Increase crit chance
              └─ Fully heal player
```

**Level Scaling**:
- Health: +10 per level
- Mana: +5 per level
- Damage: +2 per level
- Armor: +1 per level
- Crit Chance: +0.5% per level

### 8. Inventory System

**Data Flow**:
```
InventoryManager (Data Layer)
  └─ List<Item> items
  └─ OnInventoryChanged event
      └─ InventoryUI (Presentation Layer)
          └─ InventorySlotUI × 20
              └─ Display icon, rarity color, name
```

**Operations**:
- `AddItem(item)`: Add to inventory, fire event
- `RemoveItem(item)`: Remove from inventory, fire event
- `EquipItem(item)`: Call PlayerStats.Equip, fire event
- `GetItems()`: Return all items

## Data Flow Diagrams

### Complete Combat Loop
```
Player Attack Input
  ↓
ThirdPersonController.Attack()
  ↓
ProceduralAnimationController.PlayAttack(weaponType)
  ├─ Animate bone rotations
  ├─ Wind up → Swing → Follow through
  └─ OnAttackHitFrame() callback
      ↓
PlayerWeaponHandler.DealDamage() OR RangedWeaponHandler.FireProjectile()
  ↓
EnemyStats.TakeDamage(damage, isCrit)
  ├─ Armor reduction: actualDamage = damage × (1 - armorPercent)
  ├─ DamageText.Show(actualDamage, isCrit)
  └─ If health <= 0:
      ↓
      EnemyStats.Die()
        ├─ ExperienceManager.GainExperience(xpAmount)
        │   └─ Check level up
        │       └─ PlayerStats.OnLevelUp()
        └─ LootDrop.Create(item)
            └─ Collectible spawns at enemy position
```

### Enemy Spawning Loop
```
GameManager.Update()
  └─ If time >= nextSpawnTime AND enemyCount < maxEnemies:
      ↓
      GameManager.SpawnRandomEnemy()
        ├─ Calculate random position around player
        ├─ Determine enemy level based on player level
        └─ makeItems.SpawnEnemy(position, level)
            ├─ Generate procedural rig
            ├─ Generate weapon
            ├─ Attach weapon to hand bone
            └─ Add to activeEnemies list
```

### Item Generation → Equipment → Combat Loop
```
makeItems.GenerateWeapon(playerLevel)
  ├─ Determine rarity
  ├─ Calculate stats based on level + rarity
  ├─ ProceduralWeaponModels.GenerateWeaponModel()
  └─ ProceduralIconGenerator.GenerateIcon()
      ↓
      Item added to world or inventory
        ↓
        Player picks up or equips from inventory
          ↓
          PlayerStats.EquipWeapon(weapon)
            ├─ Attach model to ProceduralCharacterRig.GetWeaponBone()
            ├─ Update CombatAnimationController
            └─ Recalculate total damage
              ↓
              Next attack uses new weapon stats
```

## Integration Points

### Critical Dependencies

1. **ProceduralCharacterRig → All Character Systems**
   - Provides bone hierarchy for equipment attachment
   - Required by PlayerStats, EnemyStats for model placement
   - Used by ProceduralAnimationController for animation

2. **ProceduralAnimationController → Combat Systems**
   - Signals attack hit frames to weapon handlers
   - Synchronizes animation with damage application
   - Provides weapon-specific motion

3. **makeItems → Game Manager**
   - Character spawning (SpawnPlayer, SpawnEnemy)
   - Item generation (GenerateWeapon, GenerateArmor)
   - Singleton pattern for global access

4. **EnemyStats → ExperienceManager**
   - Awards XP on death
   - Triggers level up checks
   - Critical for progression loop

5. **PlayerStats → InventoryManager**
   - Equipment stat application
   - Armor/weapon bonuses
   - Health/mana calculations

6. **InventoryManager → InventoryUI**
   - Event-driven updates (OnInventoryChanged)
   - UI reflects data layer state
   - Click handlers trigger equip/use

### Event System

**Events**:
- `InventoryManager.OnInventoryChanged` → InventoryUI updates
- `PlayerStats.OnDeath` → Game Manager (respawn/game over)
- `PlayerStats.OnTakeDamage` → UI feedback (damage flash, shake)
- `ExperienceManager.OnLevelUp` → PlayerStats (stat increases)
- `ProceduralAnimationController.OnAttackHitFrame` → WeaponHandler (damage)

**Pattern**:
```csharp
// Publisher
public event System.Action<int, int> OnInventoryChanged; // (itemCount, capacity)
OnInventoryChanged?.Invoke(items.Count, maxCapacity);

// Subscriber
void Start() {
    inventoryManager.OnInventoryChanged += UpdateInventoryUI;
}
void UpdateInventoryUI(int count, int capacity) {
    // Update UI
}
```

## Extension Points

### Adding New Weapon Types

1. **Add enum value**:
```csharp
public enum WeaponType { 
    Sword, Axe, Mace, Dagger, Staff, Bow, Spear, Hammer, Flail
}
```

2. **Add generation logic** (`makeItems.cs`):
```csharp
case WeaponType.Spear:
    weapon.range = 3f;
    weapon.attackSpeed = 1.1f;
    break;
```

3. **Add model creation** (`ProceduralWeaponModels.cs`):
```csharp
case WeaponType.Spear:
    CreateSpearModel(weapon);
    break;
```

4. **Add animation** (`ProceduralAnimationController.cs`):
```csharp
case WeaponType.Spear:
    AnimateSpearThrust(progress);
    break;
```

### Adding New Animation States

1. **Define state** in `ProceduralAnimationController.cs`:
```csharp
private bool isJumping = false;

void UpdateJumpAnimation() {
    float jumpHeight = Mathf.Sin(jumpProgress * Mathf.PI);
    pelvis.localPosition = Vector3.up * (1 + jumpHeight * 2);
    
    // Tuck legs
    leftUpperLeg.localRotation = Quaternion.Euler(-45, 0, 0);
    rightUpperLeg.localRotation = Quaternion.Euler(-45, 0, 0);
}
```

2. **Trigger from input**:
```csharp
if (Input.GetKeyDown(KeyCode.Space)) {
    animator.PlayJump();
}
```

### Adding New Armor Types

1. **Add to ArmorType enum**:
```csharp
public enum ArmorType { 
    Helmet, Chestplate, Leggings, Gloves, Boots, Shield, Cape, Belt
}
```

2. **Map to bone** in `ProceduralCharacterRig.cs`:
```csharp
armorBones[ArmorType.Cape] = spine; // Back attachment
armorBones[ArmorType.Belt] = pelvis;
```

3. **Add generation** in `makeItems.cs` and `ProceduralArmorModels.cs`

### Adding Ragdoll Physics

```csharp
void EnableRagdoll() {
    // Disable animation
    ProceduralAnimationController anim = GetComponent<ProceduralAnimationController>();
    anim.enabled = false;
    
    // Enable physics on all bones
    Rigidbody[] bones = GetComponentsInChildren<Rigidbody>();
    foreach (Rigidbody rb in bones) {
        rb.isKinematic = false;
        rb.useGravity = true;
    }
    
    // Add joints between bones
    ConfigurableJoint joint = rightUpperArm.AddComponent<ConfigurableJoint>();
    joint.connectedBody = rightShoulder.GetComponent<Rigidbody>();
    // ... configure joint limits
}
```

### Adding IK (Inverse Kinematics)

```csharp
void LookAtTarget(Transform target) {
    // Head looks at target
    head.LookAt(target);
    
    // Constrain rotation
    Vector3 angles = head.localEulerAngles;
    angles.x = Mathf.Clamp(angles.x, -45, 45);
    angles.y = Mathf.Clamp(angles.y, -60, 60);
    head.localEulerAngles = angles;
}

void ReachForObject(Transform target) {
    // Two-joint IK for arm
    SolveTwoJointIK(rightShoulder, rightUpperArm, rightLowerArm, rightHand, target.position);
}
```

## Performance Considerations

### Current Optimizations
1. **Capsule Colliders**: Most efficient Unity primitive
2. **Code-based Animation**: Faster than Animator state machine
3. **Kinematic Rigidbodies**: No physics calculations when not needed
4. **Event-driven UI**: Updates only on inventory changes, not every frame
5. **Procedural Generation**: One-time cost, then cached

### Bottlenecks
1. **Enemy Count**: Main performance factor (10-20 optimal)
2. **Bone Count**: 22 bones × N enemies (consider LOD for distant enemies)
3. **Projectile Collisions**: Use object pooling
4. **FindObjectOfType**: Cache references in Awake/Start

### Optimization Opportunities
1. **Object Pooling**: For projectiles, enemies, damage text
2. **LOD System**: Simplify distant enemy rigs (fewer bones)
3. **Occlusion Culling**: Disable off-screen enemy rendering
4. **Spatial Partitioning**: Only update nearby enemies
5. **Bone Instancing**: Share mesh materials across characters

### Memory Profile
- **Per Character**: ~50KB (22 bones + components)
- **10 Characters**: ~500KB
- **Traditional Rigged Model**: 2-10MB per character
- **Savings**: 20-100× less memory usage

## Testing Strategy

### Unit Test Areas
1. **Rig Generation**: Verify 22 bones created with correct hierarchy
2. **Equipment Attachment**: Confirm items attach to correct bones
3. **Stat Calculations**: Test damage/armor formulas
4. **XP System**: Verify level-up thresholds
5. **Item Generation**: Check rarity distribution

### Integration Tests
1. **Full Spawn**: Player + 5 enemies spawn correctly
2. **Combat Loop**: Player kills enemy → XP gained → loot drops
3. **Equipment**: Pick up item → equip → stats update → model visible
4. **Animation**: Idle → Walk → Attack state transitions

### Performance Benchmarks
- 10 characters: 60 FPS minimum
- 20 characters: 45 FPS minimum
- 50 characters: 30 FPS minimum
- Spawn character: <100ms
- Generate weapon: <10ms

## Debugging Tools

### GameManager Context Menu
```
Right-click GameManager in Inspector:
- Spawn Test Enemy: Single enemy at player location
- Spawn 5 Enemies: Quick combat testing
- Clear All Enemies: Reset combat area
- Give Player Items: 5 random items for testing
```

### Visual Debugging
```
Scene View Gizmos:
- Yellow spheres: Bone positions
- Cyan lines: Bone hierarchy connections
- Red/Yellow circles: Enemy spawn radius
- Green sphere: Weapon attach point
```

### Console Logging
```
Key messages:
✅ Character rig generated with 22 bones
✅ Spawned procedural player at (x, y, z)
✅ Attached weapon to RightHand
⚠️ Weapon bone not found!
❌ Character rig missing!
```

## Future Architecture Improvements

1. **Component-based Entities**: Decouple systems further
2. **Save/Load System**: Serialize rig state and equipment
3. **Networked Multiplayer**: Sync bone rotations across clients
4. **Scriptable Objects**: Data-driven item/enemy configurations
5. **Asset Bundles**: On-demand loading for large worlds
6. **Job System**: Multi-threaded animation calculations
7. **ECS Architecture**: Ultra-high performance with many entities

---

**Last Updated**: Procedural Character Rig System implementation
**Version**: 2.0.0 - Complete refactor with capsule-based bones