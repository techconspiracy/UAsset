# System Architecture

## Component Hierarchy

Scene Root
├── GameManager
│   ├── makeItems (Item Generator)
│   ├── ProceduralWeaponModels
│   ├── ProceduralArmorModels
│   └── ProjectileSystem
│
├── Player
│   ├── CharacterController (Unity)
│   ├── PlayerInput (Unity)
│   ├── Animator (Unity)
│   ├── PlayerStats
│   ├── ExperienceManager
│   ├── InventoryManager
│   ├── ThirdPersonController
│   ├── CombatAnimationController
│   ├── RangedWeaponHandler
│   └── GameTester (Debug only)
│
├── Enemies
│   ├── CharacterController (Unity)
│   ├── Animator (Unity - optional)
│   ├── EnemyStats
│   └── EnemyWeaponHandler (optional)
│
└── UI
├── Canvas
├── InventoryPanel (InventoryUI)
├── PlayerStatsUI
└── ExperienceUI

## Data Flow

### Combat Loop
Player attacks
→ ThirdPersonController.Attack()
→ CombatAnimationController.PerformAttack()
→ RangedWeaponHandler.TryRangedAttack() OR melee damage
→ ProjectileSystem.FireProjectile() (if ranged)
→ Projectile.OnTriggerEnter()
→ EnemyStats.TakeDamage()
→ EnemyStats.Die() (if health <= 0)
→ ExperienceManager.GainExperience()
→ PlayerStats.OnLevelUp() (if threshold met)

### Item Generation Flow
Request item
→ makeItems.GenerateWeapon/Armor()
→ ProceduralWeaponModels.GenerateWeaponModel()
→ ProceduralIconGenerator.GenerateIcon()
→ Item created with stats and model
→ InventoryManager.AddItem()
→ InventoryUI updates

### Equipment Flow
Player selects item in UI
→ InventoryUI.OnSlotClicked()
→ InventoryManager.EquipItem()
→ PlayerStats.EquipWeapon/EquipArmor()
→ CombatAnimationController.EquipWeapon()
→ RangedWeaponHandler.SetEquippedWeapon()
→ Weapon model attached to hand bone
→ Stats recalculated

## Integration Points

### Critical Dependencies
- EnemyStats → PlayerStats (for XP awards)
- ThirdPersonController → CombatAnimationController (for animations)
- PlayerStats → makeItems (for item generation)
- RangedWeaponHandler → ProjectileSystem (for ranged combat)

### Event System
- InventoryManager.OnInventoryChanged → InventoryUI
- PlayerStats.OnDeath → Game Manager
- PlayerStats.OnTakeDamage → UI feedback
- ExperienceManager.OnLevelUp → PlayerStats

## Extension Points

### Adding New Weapon Types
1. Add enum value to WeaponType
2. Add generation logic in makeItems.GenerateWeapon()
3. Add model creation in ProceduralWeaponModels
4. Add animation in CombatAnimationController
5. (Optional) Add projectile type if ranged

### Adding New Enemy Types
1. Create enemy prefab with EnemyStats component
2. Configure stats in inspector
3. Assign loot table settings
4. (Optional) Add EnemyWeaponHandler for armed enemies

### Adding UI Elements
1. Create UI in Canvas
2. Subscribe to relevant events (OnInventoryChanged, etc.)
3. Update UI in event callbacks
4. Reference from relevant manager

## Performance Considerations

### Optimization Opportunities
- Object pooling for projectiles
- Weapon model instancing
- UI batching
- Level of detail for distant enemies

### Current Bottlenecks
- FindObjectOfType calls (cache references instead)
- Projectile collision checks (use triggers)
- UI updates every frame (event-driven instead)