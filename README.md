# UAsset Game System Documentation

## Overview
Third-person action RPG with procedural loot generation, combat, and progression systems.

## Core Systems
- **Item Generation**: Procedural weapons and armor with rarity tiers
- **Combat**: Melee and ranged weapon support with critical hits
- **Progression**: Experience-based leveling with stat scaling
- **Inventory**: Drag-and-drop UI with equipment system
- **Animation**: Combat animation controller (requires Animator setup)

## Quick Start
1. Attach makeItems, ProceduralWeaponModels, ProceduralArmorModels, and ProjectileSystem to GameManager
2. Attach all player scripts to Player GameObject
3. Set up Animator Controller (see Animation Setup below)
4. Press F1 in-game to open debug menu (GameTester)

## Dependencies
- Unity 2021.3+ (LTS recommended)
- TextMeshPro package
- New Input System package

## Animation Setup
Create Animator Controller with parameters:
- Speed (float)
- Grounded (bool)
- Jump (trigger)
- FreeFall (bool)
- Attack (trigger)
- AttackType (int)
- IsAttacking (bool)
- AttackSpeed (float)

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