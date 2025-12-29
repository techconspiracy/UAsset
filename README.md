# UAsset - Procedural RPG Game System

A complete Unity game framework featuring **fully procedural character generation** where every bone is a capsule with collider and mesh renderer. No imported models or animations required!

## 🌟 Key Features

### Procedural Character System (NEW!)
- **22-bone skeleton**: Complete hierarchy from head to feet
- **Capsule-based bones**: Each bone is a GameObject with CapsuleCollider + MeshRenderer
- **Physics-ready**: All bones have Rigidbody for proper collision handling
- **Pure code animation**: No Unity Animator needed - bones rotate via Transform
- **Automatic spawning**: Characters generate on-the-fly with complete rigs

### Game Systems
- **Procedural Item Generation**: Weapons, armor, and collectibles with dynamic stats
- **Combat System**: Melee and ranged combat with weapon-specific animations
- **Experience & Leveling**: XP gain from enemies, level-up stat bonuses
- **Inventory System**: Full item management with equipment slots
- **Dynamic Enemy Spawning**: Continuous enemy generation with level scaling

### Procedural Content
- **Weapon Models**: 6 weapon types (Sword, Axe, Mace, Dagger, Staff, Bow)
- **Armor Models**: 6 armor pieces (Helmet, Chestplate, Leggings, Gloves, Boots, Shield)
- **Item Icons**: Auto-generated sprites for inventory UI
- **Character Rigs**: Complete skeletons generated at runtime
- **Animations**: Walk, idle, and attack animations via code

## 🎮 Quick Start

### Minimum Setup (3 Steps!)

1. **Create Empty Scene**
   ```
   File → New Scene
   ```

2. **Add GameManager**
   ```
   GameObject → Create Empty → Name: "GameManager"
   Add Component → GameManager
   ```

3. **Press Play!**
   - Player spawns with procedural rig
   - 5 enemies spawn with rigs and weapons
   - Fully playable game ready

### Controls
- **WASD**: Move
- **Mouse**: Look around
- **Left Click**: Attack
- **I**: Toggle inventory
- **E**: Pick up items
- **1-6**: Quick equip slots

## 📁 Project Structure

```
Assets/Scripts/
├── Core Systems
│   ├── GameManager.cs              # Game orchestration & spawning
│   ├── makeItems.cs                # Item generation & character spawning
│   └── Architecture.md             # System documentation
│
├── Character Rig System (NEW!)
│   ├── ProceduralCharacterRig.cs           # 22-bone skeleton generation
│   └── ProceduralAnimationController.cs    # Code-based animation
│
├── Player Systems
│   ├── PlayerStats.cs              # Health, damage, armor, equipment
│   ├── ThirdPersonController.cs    # Movement and input
│   ├── CombatAnimationController.cs # Combat actions
│   ├── PlayerWeaponHandler.cs      # Melee combat
│   ├── RangedWeaponHandler.cs      # Ranged combat
│   ├── InventoryManager.cs         # Item storage
│   └── ExperienceManager.cs        # XP and leveling
│
├── Enemy Systems
│   ├── EnemyStats.cs               # Enemy health & combat
│   └── EnemyWeaponHandler.cs       # Enemy attacks
│
├── Item Systems
│   ├── ProceduralWeaponModels.cs   # Weapon mesh generation
│   ├── ProceduralArmorModels.cs    # Armor mesh generation
│   └── ProceduralIconGenerator.cs  # UI icon generation
│
├── UI Systems
│   ├── InventoryUI.cs              # Inventory interface
│   ├── InventorySlotUI.cs          # Individual slots
│   └── DamageText.cs               # Floating damage numbers
│
├── World Systems
│   ├── LootDrop.cs                 # World item pickups
│   ├── CollectibleManager.cs       # Collectible tracking
│   └── ThirdPersonCamera.cs        # Camera controller
│
└── Utilities
    ├── ProjectileSystems.cs        # Arrows and projectiles
    └── gameTester.cs               # Debug tools
```

## 🦴 Character Rig System

### Bone Hierarchy (22 Bones)

```
Root
└─ Pelvis
   ├─ Spine
   │  └─ Chest
   │     ├─ Neck
   │     │  └─ Head
   │     ├─ LeftShoulder
   │     │  └─ LeftUpperArm
   │     │     └─ LeftLowerArm
   │     │        └─ LeftHand
   │     └─ RightShoulder
   │        └─ RightUpperArm
   │           └─ RightLowerArm
   │              └─ RightHand (weapon attachment)
   ├─ LeftHip
   │  └─ LeftUpperLeg
   │     └─ LeftLowerLeg
   │        └─ LeftFoot
   └─ RightHip
      └─ RightUpperLeg
         └─ RightLowerLeg
            └─ RightFoot
```

### Each Bone Contains
- **CapsuleCollider**: Physics collision detection
- **MeshRenderer**: Visual capsule mesh
- **Rigidbody**: Kinematic physics body
- **Transform**: Position, rotation, scale hierarchy

### Animation States
- **Idle**: Breathing motion, head turning, subtle swaying
- **Walk**: Leg/arm swing, vertical bobbing, forward lean
- **Attack**: Weapon-specific (sword swing, dagger stab, bow draw, staff cast)

## 🎯 System Architecture

### Character Spawn Flow
```
GameManager.StartGame()
  └─ makeItems.SpawnPlayer()
      ├─ Create GameObject
      ├─ Add CharacterController
      ├─ Add ProceduralCharacterRig
      │   └─ GenerateCompleteRig() → Creates 22 bones
      ├─ Add ProceduralAnimationController
      ├─ Add PlayerStats
      ├─ Add Combat Systems
      └─ Add Inventory & Experience
```

### Combat Flow
```
Player Input
  └─ ThirdPersonController.Attack()
      └─ ProceduralAnimationController.PlayAttack()
          └─ Animate weapon swing via bone rotation
              └─ PlayerWeaponHandler.OnAttackHitFrame()
                  └─ EnemyStats.TakeDamage()
                      └─ EnemyStats.Die()
                          ├─ ExperienceManager.GainExperience()
                          └─ LootDrop.Create()
```

### Item Generation Flow
```
makeItems.GenerateWeapon()
  ├─ Determine rarity & stats
  ├─ Generate procedural name
  ├─ ProceduralWeaponModels.GenerateWeaponModel()
  └─ ProceduralIconGenerator.GenerateIcon()
      └─ Return Weapon with model & stats
```

### Equipment Flow
```
InventoryUI.OnItemClick()
  └─ PlayerStats.EquipWeapon()
      └─ Attach to ProceduralCharacterRig.GetWeaponBone()
          └─ weapon.transform.SetParent(rightHand)
              └─ ProceduralAnimationController updates attack animation
```

## 🛠️ Customization

### Modify Bone Dimensions
```csharp
// In ProceduralCharacterRig Inspector
Head Radius: 0.15
Torso Radius: 0.18
Upper Arm Length: 0.3
Lower Arm Length: 0.25
Upper Leg Length: 0.4
Lower Leg Length: 0.4
Body Scale: 1.0  // Scales entire rig
```

### Change Character Colors
```csharp
ProceduralCharacterRig rig = GetComponent<ProceduralCharacterRig>();
rig.SetSkinColor(Color.green); // Make character green
```

### Add Custom Animations
```csharp
// In ProceduralAnimationController
void AnimateCustomAction() {
    // Rotate bones to create animation
    rightUpperArm.localRotation = Quaternion.Euler(90, 0, 0);
    leftUpperArm.localRotation = Quaternion.Euler(-90, 0, 0);
}
```

### Create New Weapon Type
```csharp
// 1. Add to WeaponType enum
public enum WeaponType { 
    Sword, Axe, Mace, Dagger, Staff, Bow, Spear // NEW!
}

// 2. Add generation in makeItems.cs
weapon.weaponType = (WeaponType)UnityEngine.Random.Range(0, 7);

// 3. Add model in ProceduralWeaponModels.cs
case WeaponType.Spear:
    CreateSpearModel(weapon);
    break;

// 4. Add animation in ProceduralAnimationController.cs
case WeaponType.Spear:
    AnimateSpearThrust(progress);
    break;
```

## 📊 Performance

### Optimizations
- **Lightweight Characters**: ~50KB per character vs MBs for traditional models
- **Efficient Animation**: Code-based rotation faster than Animator state machine
- **Object Pooling**: Recommended for projectiles (not yet implemented)
- **Bone Colliders**: Capsules are Unity's most efficient primitive

### Benchmarks (Approximate)
- **10 Characters**: 60 FPS on mid-range PC
- **20 Characters**: 45-50 FPS
- **50 Characters**: 25-30 FPS
- **Memory**: ~500KB for 10 complete characters with equipment

### Bottlenecks
- Enemy count is main performance factor
- Consider LOD system for distant enemies
- Pool projectiles if using many ranged weapons

## 🐛 Debug Tools

### GameManager Context Menu
```
Right-click GameManager in Inspector:
- Spawn Test Enemy
- Spawn 5 Enemies
- Clear All Enemies
- Give Player Items
```

### Console Commands
```csharp
// In any script
GameManager.Instance.SpawnEnemyAt(position, level);
GameManager.Instance.ClearAllEnemies();
GameManager.Instance.RestartGame();
```

### Visual Debugging
- Enable Gizmos in Scene view to see bone structure
- Yellow spheres = bone positions
- Cyan lines = bone connections
- Red/Yellow circles = enemy spawn radius

## 📚 Documentation

- **Architecture.md**: Complete system architecture and data flow
- **INTEGRATION_GUIDE.md**: Detailed implementation guide with examples
- Inline code comments explain all major systems

## 🔮 Roadmap

### Planned Features
- [ ] Ragdoll physics on death (disable kinematic Rigidbodies)
- [ ] IK system for ground adaptation and object interaction
- [ ] More animation states (jump, roll, climb)
- [ ] Procedural facial features on head bone
- [ ] Armor visible on character (layered meshes)
- [ ] Enemy AI pathfinding and behaviors
- [ ] Save/load system for character progression
- [ ] Multiplayer networking support

### Possible Improvements
- [ ] Blend shapes for facial animation
- [ ] Cloth physics for capes/robes
- [ ] Particle effects for abilities
- [ ] Sound effects and music
- [ ] Quest system
- [ ] Crafting system

## 🤝 Contributing

This is a learning project demonstrating procedural game systems. Feel free to:
- Fork and experiment
- Submit issues for bugs
- Suggest new features
- Share your modifications

## 📝 License

This project is provided as-is for educational purposes. Use freely in your own projects!

## 🙏 Acknowledgments

Built with Unity 2022+ using only built-in packages. No external dependencies required.

---

**Ready to create characters from nothing?** Clone the repo and press play! 🎮✨

## Script List
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/scriptList.txt
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/Architecture.md
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/cameraInputRelay.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/CollectibleManager.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/CollectibleObject.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/CombatAnimationController.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ComponentVerificationTest.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/DamageText.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/EnemyStats.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/EnemyWeaponHandler.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ExperienceManager.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/gameTester.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/InventoryManager.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/InventorySlotUI.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/InventoryUI.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/LootDrop.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/makeItems.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/PlayerStats.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/PlayerWeaponHandler.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ProceduralArmorModels.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ProceduralIconGenerator.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ProceduralWeaponModels.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ProjectileSystems.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/RangedWeaponHandler.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ThirdPersonCamera.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ThirdPersonController.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/WeaponGenerationDebug.cs
https://raw.githubusercontent.com/techconspiracy/UAsset/refs/heads/main/Assets/Scripts/ProceduralCharacterRig.cs

## Bug Reports
Please use GitHub Issues for bug reports and feature requests.