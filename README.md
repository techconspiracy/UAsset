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

## Bug Reports
Please use GitHub Issues for bug reports and feature requests.