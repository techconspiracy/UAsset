using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Systems
{
    /// <summary>
    /// Central hub for all core game systems.
    /// Manages initialization order and system dependencies.
    /// DontDestroyOnLoad singleton - persists across scenes.
    /// </summary>
    public class CoreSystemManager : MonoBehaviour
    {
        public static CoreSystemManager Instance { get; private set; }
        
        [Header("System References (Auto-Assigned at Runtime)")]
        [SerializeField] private ObjectPoolManager _poolManager;
        [SerializeField] private ZoneSystemManager _zoneManager;
        [SerializeField] private EntitySystemManager _entityManager;
        [SerializeField] private CombatSystemManager _combatManager;
        [SerializeField] private InventorySystemManager _inventoryManager;
        [SerializeField] private UISystemManager _uiManager;
        [SerializeField] private AudioSystemManager _audioManager;
        [SerializeField] private WebSocketNetworkManager _networkManager;
        [SerializeField] private AdminConsoleManager _adminConsoleManager;
        
        [Header("Initialization Status")]
        [SerializeField] private bool _isInitialized;
        [SerializeField] private float _initializationTime;
        
        // Public accessors for systems
        public static ObjectPoolManager PoolManager => Instance?._poolManager;
        public static ZoneSystemManager ZoneManager => Instance?._zoneManager;
        public static EntitySystemManager EntityManager => Instance?._entityManager;
        public static CombatSystemManager CombatManager => Instance?._combatManager;
        public static InventorySystemManager InventoryManager => Instance?._inventoryManager;
        public static UISystemManager UIManager => Instance?._uiManager;
        public static AudioSystemManager AudioManager => Instance?._audioManager;
        public static WebSocketNetworkManager NetworkManager => Instance?._networkManager;
        public static AdminConsoleManager AdminConsole => Instance?._adminConsoleManager;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton enforcement
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region System Initialization
        
        private void InitializeSystems()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[CoreSystemManager] Initializing core systems...");
            
            // Initialize systems in dependency order
            InitializePoolManager();
            InitializeNetworkManager();
            InitializeZoneManager();
            InitializeEntityManager();
            InitializeCombatManager();
            InitializeInventoryManager();
            InitializeAudioManager();
            InitializeUIManager();
            InitializeAdminConsole();
            
            _initializationTime = Time.realtimeSinceStartup - startTime;
            _isInitialized = true;
            
            Debug.Log($"[CoreSystemManager] All systems initialized in {_initializationTime:F3}s");
        }
        
        private void InitializePoolManager()
        {
            _poolManager = GetOrCreateChildSystem<ObjectPoolManager>("ObjectPoolManager");
        }
        
        private void InitializeNetworkManager()
        {
            _networkManager = GetOrCreateChildSystem<WebSocketNetworkManager>("WebSocketNetworkManager");
        }
        
        private void InitializeZoneManager()
        {
            _zoneManager = GetOrCreateChildSystem<ZoneSystemManager>("ZoneSystemManager");
        }
        
        private void InitializeEntityManager()
        {
            _entityManager = GetOrCreateChildSystem<EntitySystemManager>("EntitySystemManager");
        }
        
        private void InitializeCombatManager()
        {
            _combatManager = GetOrCreateChildSystem<CombatSystemManager>("CombatSystemManager");
        }
        
        private void InitializeInventoryManager()
        {
            _inventoryManager = GetOrCreateChildSystem<InventorySystemManager>("InventorySystemManager");
        }
        
        private void InitializeAudioManager()
        {
            _audioManager = GetOrCreateChildSystem<AudioSystemManager>("AudioSystemManager");
        }
        
        private void InitializeUIManager()
        {
            _uiManager = GetOrCreateChildSystem<UISystemManager>("UISystemManager");
        }
        
        private void InitializeAdminConsole()
        {
            _adminConsoleManager = GetOrCreateChildSystem<AdminConsoleManager>("AdminConsoleManager");
        }
        
        /// <summary>
        /// Helper method to get or create a child system component.
        /// </summary>
        private T GetOrCreateChildSystem<T>(string childName) where T : Component
        {
            // Check if already exists as child
            Transform child = transform.Find(childName);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    Debug.Log($"[CoreSystemManager] Found existing {typeof(T).Name}");
                    return component;
                }
            }
            
            // Create new child object with component
            GameObject systemObj = new(childName);
            systemObj.transform.SetParent(transform);
            T newComponent = systemObj.AddComponent<T>();
            
            Debug.Log($"[CoreSystemManager] Created {typeof(T).Name}");
            return newComponent;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Check if all core systems are initialized and ready.
        /// </summary>
        public bool IsReady()
        {
            return _isInitialized 
                && _poolManager != null
                && _zoneManager != null
                && _entityManager != null
                && _combatManager != null
                && _inventoryManager != null
                && _uiManager != null
                && _audioManager != null
                && _networkManager != null
                && _adminConsoleManager != null;
        }
        
        /// <summary>
        /// Shutdown all systems gracefully.
        /// Call before application quit or when switching to main menu.
        /// </summary>
        public async Awaitable ShutdownAllSystems()
        {
            Debug.Log("[CoreSystemManager] Shutting down all systems...");
            
            // Shutdown in reverse order
            if (_adminConsoleManager != null) 
                _adminConsoleManager.Shutdown();
            
            if (_uiManager != null) 
                _uiManager.Shutdown();
            
            if (_audioManager != null) 
                _audioManager.Shutdown();
            
            if (_inventoryManager != null) 
                _inventoryManager.Shutdown();
            
            if (_combatManager != null) 
                _combatManager.Shutdown();
            
            if (_entityManager != null) 
                await _entityManager.ShutdownAsync();
            
            if (_zoneManager != null) 
                await _zoneManager.ShutdownAsync();
            
            if (_networkManager != null) 
                await _networkManager.DisconnectAsync();
            
            if (_poolManager != null)
                _poolManager.ClearAllPools();
            
            _isInitialized = false;
            
            Debug.Log("[CoreSystemManager] All systems shut down");
        }
        
        #endregion
    }
    
    #region Placeholder System Classes (To be implemented)
    
    /// <summary>
    /// Manages zone loading, unloading, and scene caching.
    /// </summary>
    public class ZoneSystemManager : MonoBehaviour
    {
        public void Shutdown() { }
        public async Awaitable ShutdownAsync() 
        { 
            await Awaitable.NextFrameAsync();
        }
    }
    
    /// <summary>
    /// Manages all entities (players, NPCs, enemies).
    /// </summary>
    public class EntitySystemManager : MonoBehaviour
    {
        public async Awaitable ShutdownAsync() 
        { 
            await Awaitable.NextFrameAsync();
        }
    }
    
    /// <summary>
    /// Handles combat calculations, damage, abilities, magic.
    /// </summary>
    public class CombatSystemManager : MonoBehaviour
    {
        public void Shutdown() { }
    }
    
    /// <summary>
    /// Manages inventory, equipment, items, loot.
    /// </summary>
    public class InventorySystemManager : MonoBehaviour
    {
        public void Shutdown() { }
    }
    
    /// <summary>
    /// Manages all UI (HUD, menus, admin console).
    /// </summary>
    public class UISystemManager : MonoBehaviour
    {
        public void Shutdown() { }
    }
    
    /// <summary>
    /// Handles audio pooling and playback.
    /// </summary>
    public class AudioSystemManager : MonoBehaviour
    {
        public void Shutdown() { }
    }
    
    /// <summary>
    /// WebSocket-based LAN multiplayer (max 6 players).
    /// </summary>
    public class WebSocketNetworkManager : MonoBehaviour
    {
        public async Awaitable DisconnectAsync() 
        { 
            await Awaitable.NextFrameAsync();
        }
    }
    
    /// <summary>
    /// Admin console with in-game editors for weapons, armor, spells, pools, etc.
    /// </summary>
    public class AdminConsoleManager : MonoBehaviour
    {
        public void Shutdown() { }
    }
    
    #endregion
}