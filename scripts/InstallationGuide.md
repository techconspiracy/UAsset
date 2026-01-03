# UAsset MMORPG System - Installation Guide

## Table of Contents
1. [System Requirements](#system-requirements)
2. [Unity Project Setup](#unity-project-setup)
3. [Script Installation](#script-installation)
4. [Scene Setup](#scene-setup)
5. [Configuration](#configuration)
6. [Verification](#verification)

---

## System Requirements

### Hardware
- **Minimum**: Samsung A15 or equivalent (4GB RAM, integrated graphics)
- **Target Performance**: 40 FPS
- **Development Machine**: Any system capable of running Unity 6.1+

### Software
- **Unity Version**: 6.1.0f1 or later
- **.NET Runtime**: .NET 8 (included with Unity 6.1+)
- **Required Unity Packages**:
  - Collections (for NativeArray/NativeList)
  - Burst Compiler
  - Unity UI (legacy, for basic UI)

---

## Unity Project Setup

### Step 1: Create New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select **3D (URP)** or **3D Core** template
4. Set project name: `UAsset_MMORPG`
5. Choose location and click "Create Project"

### Step 2: Configure Project Settings

1. Go to **Edit → Project Settings**

2. **Player Settings**:
   ```
   - Company Name: YourCompanyName
   - Product Name: UAsset MMORPG
   - Default Icon: (optional)
   - Resolution:
     * Default Screen Width: 1920
     * Default Screen Height: 1080
     * Run in Background: ✓
   ```

3. **Quality Settings**:
   ```
   - Set Default Quality Level: Medium (index 2)
   - VSync Count: Every V Blank (for testing)
   - Anti-Aliasing: 2x Multi Sampling (for low-end)
   ```

4. **Graphics Settings**:
   ```
   - Transparency Sort Mode: Orthographic
   - Shader Stripping: ✓ Strip Unused Variants
   ```

5. **Time Settings**:
   ```
   - Fixed Timestep: 0.02 (50 FPS physics)
   ```

### Step 3: Install Required Packages

1. Go to **Window → Package Manager**
2. Click **+ (Add)** → **Add package by name**
3. Install these packages:
   ```
   com.unity.burst
   com.unity.collections
   com.unity.ugui
   ```

---

## Script Installation

### Directory Structure

Create the following folder structure in your project:

```
Assets/
├── Scenes/
│   ├── Bootstrap.unity (main entry scene)
│   ├── MainMenu.unity
│   └── Zones/
│       └── (generated zones will be saved here)
├── Scripts/
│   └── Game/
│       └── Core/
│           ├── Pooling/
│           │   └── IPoolable.cs
│           └── Systems/
│               ├── GameBootstrap.cs
│               ├── CoreSystemManager.cs
│               ├── ObjectPoolManager.cs
│               ├── ZoneSceneManager.cs
│               ├── SimpleTerrainGenerator.cs
│               ├── InteractableRegistry.cs
│               ├── ProceduralCharacterBuilder.cs
│               ├── ItemGenerationEngine.cs
│               ├── CombatSystem.cs
│               ├── UISystem.cs
│               ├── AdminConsoleManager.cs
│               └── WebSocketNetworkManager.cs
└── Prefabs/
    └── (will be created during setup)
```

### Step-by-Step Script Installation

1. **Create the directory structure** as shown above

2. **Copy each script** from the artifacts into the appropriate folder:
   - `IPoolable.cs` → `Assets/Scripts/Game/Core/Pooling/`
   - All other `.cs` files → `Assets/Scripts/Game/Core/Systems/`

3. **Verify compilation**:
   - Unity should automatically compile the scripts
   - Check the Console window (Ctrl+Shift+C) for any errors
   - If errors appear, ensure all namespaces are correct

4. **Wait for compilation** to complete (status bar bottom-right)

---

## Scene Setup

### Create Bootstrap Scene

This is the most important setup step.

1. **Create new scene**:
   - File → New Scene
   - Save as `Assets/Scenes/Bootstrap.unity`

2. **Create Core GameObject**:
   - Right-click in Hierarchy
   - Create Empty
   - Name it: `GameBootstrap`
   - Position: (0, 0, 0)

3. **Add GameBootstrap Component**:
   - Select `GameBootstrap` GameObject
   - In Inspector, click "Add Component"
   - Search for "Game Bootstrap"
   - Click to add the script

4. **Configure GameBootstrap**:
   ```
   Bootstrap Configuration:
   - Initial Scene Name: "MainMenu"
   - Show Loading Screen: ✓
   - Min Loading Time: 1

   Performance Settings:
   - Target Frame Rate: 60
   - Enable VSync: ✓

   Quality Settings:
   - Default Quality Level: 2 (Medium)
   ```

5. **Save the scene**: Ctrl+S

### Create MainMenu Scene

1. **Create new scene**:
   - File → New Scene
   - Save as `Assets/Scenes/MainMenu.unity`

2. **Add basic UI**:
   - Right-click Hierarchy → UI → Canvas
   - Right-click Canvas → UI → Text
   - Set text to "Main Menu - Press SPACE to start"
   - Position text in center of canvas

3. **Add temporary start script**:
   ```csharp
   // Create Scripts/MainMenuController.cs
   using UnityEngine;
   using UnityEngine.SceneManagement;

   public class MainMenuController : MonoBehaviour
   {
       void Update()
       {
           if (Input.GetKeyDown(KeyCode.Space))
           {
               // Will be replaced with proper character creation
               Debug.Log("Starting game...");
           }
       }
   }
   ```

4. **Add script to scene**:
   - Create empty GameObject named "MainMenuController"
   - Add the script component
   - Save scene

### Configure Build Settings

1. Go to **File → Build Settings**
2. Click **Add Open Scenes** with Bootstrap scene open
3. Switch to MainMenu scene, click **Add Open Scenes**
4. Ensure **Bootstrap** is scene index 0 (top of list)
5. Platform: PC, Mac & Linux Standalone (or target platform)
6. Click **Close** (don't build yet)

---

## Configuration

### Object Pool Configuration

1. **Open Bootstrap scene**
2. **Select GameBootstrap GameObject**
3. It will automatically create CoreSystemManager as a child
4. **Select CoreSystemManager → ObjectPoolManager (child)**
5. **Configure Pre-Warm Pools**:

   Create pooled prefabs first:

#### Creating Pooled Prefabs

**DamageNumber Prefab**:
1. Create Empty GameObject named "DamageNumber"
2. Add `DamageNumber` component (from CombatSystem.cs)
3. Add child GameObject named "Text"
4. Add `TextMesh` component to child
5. Configure TextMesh:
   - Font Size: 32
   - Anchor: Middle Center
   - Color: White
6. Drag DamageNumber to `Assets/Prefabs/` folder
7. Delete from scene

**Back to ObjectPoolManager Configuration**:
1. Select ObjectPoolManager in Hierarchy
2. In Inspector, find "Pre Warm Pools" array
3. Set Size: 1 (for now)
4. Configure Element 0:
   ```
   Pool Key: "DamageNumber"
   Prefab: (drag DamageNumber prefab here)
   Initial Size: 50
   Max Pool Size: 100
   ```

### Zone System Configuration

1. **Create Zones folder**: `Assets/Scenes/Zones/` (already done)
2. **Select ZoneSceneManager** (child of CoreSystemManager)
3. **Configure**:
   ```
   Zone Save Path: "Assets/Scenes/Zones/"
   Max Loaded Zones: 3
   ```

---

## Verification

### Test 1: Bootstrap Flow

1. **Open Bootstrap scene**
2. **Press Play**
3. **Expected behavior**:
   - Loading screen appears
   - "GameBootstrap" logs appear in Console
   - "CoreSystemManager" initializes all systems
   - MainMenu scene loads
   - No errors in Console

**If this fails**:
- Check Console for errors
- Verify all scripts compiled successfully
- Ensure GameBootstrap component is on GameObject
- Verify MainMenu scene name matches configuration

### Test 2: Object Pooling

1. **Stop Play mode**
2. **Open Bootstrap scene**
3. **Find ObjectPoolManager in Hierarchy** (child of CoreSystemManager)
4. **Press Play**
5. **Check Inspector** (with ObjectPoolManager selected):
   - Total Pooled Objects: 50 (or your configured amount)
   - Pool Hits: 0
   - Pool Misses: 0

**If this fails**:
- Verify DamageNumber prefab exists
- Check prefab has DamageNumber component
- Verify Pool Key matches exactly

### Test 3: Admin Console

1. **Press Play** (in Bootstrap scene)
2. **Wait for MainMenu to load**
3. **Press F12**
4. **Expected behavior**:
   - Admin Console appears
   - Time pauses
   - Cursor becomes visible
   - Tab buttons visible at top

**If this fails**:
- Check if you're in single-player mode (admin always authorized)
- Verify AdminConsoleManager exists under CoreSystemManager
- Check Console for initialization errors

### Test 4: Zone Generation (Advanced)

This test requires creating a test scene.

1. **Create new scene**: `TestZoneGeneration.unity`
2. **Add empty GameObject**: "ZoneTestController"
3. **Create test script**:
   ```csharp
   using UnityEngine;
   using Game.Core.Systems;

   public class ZoneTestController : MonoBehaviour
   {
       async void Start()
       {
           // Wait for core systems
           while (CoreSystemManager.Instance == null || !CoreSystemManager.Instance.IsReady())
           {
               await Awaitable.NextFrameAsync();
           }

           // Create test zone config
           ZoneConfig config = new()
           {
               zoneName = "TestZone",
               zoneType = ZoneType.Wilderness,
               biomeType = BiomeType.Grassland,
               zoneSize = new Vector3(100, 0, 100),
               seed = 12345
           };

           // Generate zone
           await CoreSystemManager.ZoneManager.GenerateAndSaveZone(config);

           Debug.Log("Test zone generated!");
       }
   }
   ```
4. **Add script to GameObject**
5. **Press Play**
6. **Expected behavior**:
   - Terrain mesh generates
   - Rolling hills visible
   - Green grassland color
   - Boundary walls (invisible) surround zone
   - Console logs success

**If this fails**:
- Check CoreSystemManager is initialized
- Verify SimpleTerrainGenerator exists
- Check for mesh generation errors in Console

---

## Common Issues & Solutions

### Issue: "Type or namespace 'Awaitable' could not be found"
**Solution**: 
- Verify Unity version is 6.1+
- Awaitable is new in Unity 6
- For older Unity versions, use `Task` from `System.Threading.Tasks`

### Issue: "ObjectPoolManager.Instance is null"
**Solution**:
- Ensure GameBootstrap scene loads first
- Check CoreSystemManager initializes before accessing pools
- Add null checks before using pooled objects

### Issue: Scripts won't compile
**Solution**:
- Ensure all scripts are in correct folders
- Verify namespace declarations match: `Game.Core.Systems`
- Check for Unity version compatibility
- Look for missing `using` statements

### Issue: Performance below 40 FPS
**Solution**:
- Reduce terrain resolution in SimpleTerrainGenerator
- Lower Max Pool Size values
- Disable VSync and set target framerate manually
- Reduce quality settings
- Profile with Unity Profiler (Window → Analysis → Profiler)

### Issue: Zone doesn't generate
**Solution**:
- Check ZoneSceneManager initialization
- Verify SimpleTerrainGenerator is attached
- Look for errors during mesh generation
- Ensure zone save path exists and is writable

---

## Next Steps

After successful installation and verification:

1. **Read USER_MANUAL.md** for gameplay features
2. **Read API_DOCUMENTATION.md** for extending the system
3. **Study EXAMPLES.md** for implementation patterns
4. **Customize** pools, terrain, and generation settings

---

## Support & Debugging

### Enable Debug Logs

Add this to any script for detailed logging:
```csharp
#define VERBOSE_DEBUG

#if VERBOSE_DEBUG
Debug.Log($"[{GetType().Name}] Your debug message here");
#endif
```

### Unity Profiler

1. **Window → Analysis → Profiler**
2. **Press Play**
3. **Monitor**:
   - CPU Usage (should have headroom under 25ms per frame)
   - Memory (watch for GC spikes - should be zero in Update loops)
   - Rendering (check draw calls and batches)

### Console Filters

- Click the Console's filter dropdown
- Enable "Collapse" to group similar messages
- Use "Clear on Play" to see fresh logs each run

---

## Installation Checklist

- [ ] Unity 6.1+ installed
- [ ] Project created with 3D template
- [ ] Required packages installed (Burst, Collections, UI)
- [ ] All scripts copied to correct folders
- [ ] Scripts compiled without errors
- [ ] Bootstrap scene created with GameBootstrap component
- [ ] MainMenu scene created
- [ ] Build Settings configured (Bootstrap as scene 0)
- [ ] DamageNumber prefab created
- [ ] ObjectPoolManager configured with at least one pool
- [ ] Test 1 passed: Bootstrap flow works
- [ ] Test 2 passed: Object pooling working
- [ ] Test 3 passed: Admin console opens with F12
- [ ] No errors in Console during Play mode

**If all checkboxes are checked, installation is complete!**

Continue to USER_MANUAL.md for gameplay instructions.