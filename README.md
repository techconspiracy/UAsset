This document establishes the gold standard for C# code quality within the Unity 6.1+ ecosystem (.NET 8 runtime). These rules MUST be strictly followed by all AI coding agents and contributors to ensure maximum performance, security, and maintainability.
Your Core Principles
All Unity code you write MUST be "Frame-Budget Conscious" and fully optimized for the Burst Compiler where applicable.
"Fully optimized" includes:
 * Zero-Allocation Update Loops: No new keywords, string concatenations, or LINQ inside high-frequency loops.
 * Burst Compatibility: Using structs and NativeContainers for heavy math/logic to allow C# code to run at native C++ speeds.
 * Big-O for Physics & Lookups: Preferring Spatial Hashing or LayerMasks over GameObject.Find or GetComponent in every frame.
 * Modern Unity Async: Leveraging Unity 6 Awaitables for non-blocking I/O and frame-based logic.
> [!CAUTION]
> If the code is not fully optimized or results in a Garbage Collection (GC) spike before handing off to the user, you will be fined $100. You have permission to perform a "Profiling Pass" to ensure zero allocations.
> 
Preferred Tools & Frameworks (Unity 6.1 Edition)
 * Asynchronous Logic: ALWAYS use UnityEngine.Awaitable (new in Unity 6) instead of System.Threading.Tasks.Task or Coroutines. Awaitable is pooled and highly optimized for the engine loop.
 * UI System: Use UI Toolkit (UXML/USS) for all new UI. Only use uGUI (Canvas) if specifically requested for legacy support.
 * Serialization: Use JsonUtility for simple types or the Unity-optimized version of Newtonsoft JSON (available via UPM).
 * Web/API: Use UnityWebRequest for cross-platform compatibility (especially WebGL/Mobile).
 * Graphics Logic: Use Shader Graph for visuals and Compute Shaders for data-parallel tasks.
 * Data Structures: * ALWAYS use NativeArray<T>, NativeList<T>, and NativeParallelHashMap<T> (from the Collections package) for Burst-compiled logic.
   * Use ScriptableObjects for data-driven design and configuration.
Unity Code Style & Formatting
| Element | Convention | Example |
|---|---|---|
| Public Fields / Properties | PascalCase | public float MoveSpeed; |
| Serialized Private Fields | _camelCase | [SerializeField] private int _health; |
| Methods | PascalCase | public void TakeDamage(int amount) |
| Namespaces | PascalCase | namespace Game.Systems.Combat |
 * Serializability: Use [SerializeField] for private variables that need Inspector access. NEVER make a variable public just for Inspector visibility.
 * Organization: Use [Header("Settings")] and [Tooltip("...")] to make the Inspector user-friendly.
 * Naming: Methods should be verbs (ExecuteJump, not JumpManager).
High-Performance Function Design
 * The "No-New" Rule: Avoid new inside Update, FixedUpdate, or LateUpdate.
 * Component Caching: ALWAYS cache components in Awake() or Start().
 * Return Early: Use guard clauses to reduce nesting depth.
 * Burst & Jobs: For intensive calculations (e.g., pathfinding, mesh deformation), MUST implement IJobEntity or IJobParallelFor with the [BurstCompile] attribute.
Example: Optimized Async Workflow (Unity 6.1)
/// <summary>
/// Handles player data synchronization using Unity 6 Awaitables.
/// </summary>
/// <returns>Awaitable representing the operation.</returns>
[Tooltip("Syncs player data with the remote cloud service.")]
public async Awaitable SyncPlayerDataAsync(CancellationToken token)
{
    try 
    {
        // Unity 6 Awaitables are pooled; do not await the same instance twice.
        await Awaitable.NextFrameAsync(token);
        
        using var request = UnityWebRequest.Get(_syncUrl);
        await request.SendWebRequest().Awaitable;

        if (request.result != UnityWebRequest.Result.Success)
            throw new Exception($"Sync failed: {request.error}");
    }
    catch (OperationCanceledException)
    {
        Debug.Log("Sync was cancelled.");
    }
}

Memory and Performance
 * Structs over Classes: Use struct for small data containers to keep them on the stack and avoid GC pressure.
 * Object Pooling: MUST implement object pooling for frequently spawned items (bullets, particles, enemies). Never Instantiate or Destroy in a loop.
 * String Performance: Use FixedString32Bytes (or 64/128) when working within the Job System/Burst.
 * Material Access: Use Renderer.sharedMaterial when possible to avoid material instantiation, or MaterialPropertyBlock for per-instance changes.
Security
 * Secret Management: NEVER hardcode API keys or secrets in scripts. Use Unity Cloud Secrets Manager or an external .env file that is excluded via .gitignore.
 * Save Data: NEVER store sensitive save data in plain text PlayerPrefs. Use encrypted binary files or the Unity.Services.CloudSave SDK.
 * Networking: Ensure all UnityWebRequest calls use https. Validate all incoming data from authoritative servers.
Before Submitting Code
 * [ ] Profiling Check: Code runs with zero allocations in the Update loop (verified via Memory Profiler).
 * [ ] Burst Check: All Job structs are marked [BurstCompile].
 * [ ] Async Check: Used Awaitable instead of Task or yield return.
 * [ ] Inspector Check: Public/Serialized fields have [Tooltip] and [Header] attributes.
 * [ ] Cancellation Check: All async methods accept and respect a CancellationToken.
 * [ ] Namespace Check: Code is properly namespaced to avoid collisions with Unity packages.