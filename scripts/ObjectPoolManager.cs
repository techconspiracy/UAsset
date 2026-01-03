using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Pooling;

namespace Game.Core.Systems
{
    /// <summary>
    /// Centralized object pool manager for zero-allocation spawning.
    /// Handles all poolable objects in the game.
    /// CRITICAL: This system MUST have zero allocations in Get/Return operations.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }
        
        [Header("Pool Configuration")]
        [SerializeField, Tooltip("Parent transform for all pooled objects")]
        private Transform _poolRoot;
        
        [SerializeField, Tooltip("Initial pool sizes per type")]
        private List<PoolConfiguration> _preWarmPools = new();
        
        [Header("Runtime Pool Statistics (Read-Only)")]
        [SerializeField] private int _totalPooledObjects;
        [SerializeField] private int _totalActiveObjects;
        [SerializeField] private int _poolHits;
        [SerializeField] private int _poolMisses;
        
        // Pool storage: Dictionary<PoolKey, Stack<IPoolable>>
        private Dictionary<string, Stack<IPoolable>> _pools;
        
        // Prefab registry: Dictionary<PoolKey, GameObject>
        private Dictionary<string, GameObject> _prefabs;
        
        // Active tracking for admin inspection
        private Dictionary<string, HashSet<IPoolable>> _activeObjects;
        
        // Pool limits to prevent memory bloat
        private Dictionary<string, int> _poolLimits;
        
        private const int DEFAULT_POOL_LIMIT = 500;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializePools();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializePools()
        {
            _pools = new Dictionary<string, Stack<IPoolable>>(32);
            _prefabs = new Dictionary<string, GameObject>(32);
            _activeObjects = new Dictionary<string, HashSet<IPoolable>>(32);
            _poolLimits = new Dictionary<string, int>(32);
            
            // Create pool root if not assigned
            if (_poolRoot == null)
            {
                GameObject root = new("PoolRoot");
                root.transform.SetParent(transform);
                _poolRoot = root.transform;
            }
            
            // Pre-warm configured pools
            foreach (PoolConfiguration config in _preWarmPools)
            {
                if (config.prefab != null)
                {
                    RegisterPrefab(config.poolKey, config.prefab, config.maxPoolSize);
                    WarmPool(config.poolKey, config.initialSize);
                }
            }
            
            Debug.Log($"[ObjectPoolManager] Initialized with {_preWarmPools.Count} pre-warmed pools");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Register a prefab for pooling.
        /// Must be called before Get() for that pool type.
        /// </summary>
        public void RegisterPrefab(string poolKey, GameObject prefab, int maxPoolSize = DEFAULT_POOL_LIMIT)
        {
            if (string.IsNullOrEmpty(poolKey))
            {
                Debug.LogError("[ObjectPoolManager] Cannot register prefab with empty pool key!");
                return;
            }
            
            if (_prefabs.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool key '{poolKey}' already registered. Overwriting.");
            }
            
            _prefabs[poolKey] = prefab;
            _poolLimits[poolKey] = maxPoolSize;
            
            // Initialize pool structures
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new Stack<IPoolable>(maxPoolSize / 4);
            }
            
            if (!_activeObjects.ContainsKey(poolKey))
            {
                _activeObjects[poolKey] = new HashSet<IPoolable>();
            }
        }
        
        /// <summary>
        /// Pre-warm a pool with specified number of instances.
        /// Call during scene load or async to avoid runtime hitches.
        /// </summary>
        public void WarmPool(string poolKey, int count)
        {
            if (!_prefabs.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Cannot warm pool '{poolKey}' - prefab not registered!");
                return;
            }
            
            GameObject prefab = _prefabs[poolKey];
            Stack<IPoolable> pool = _pools[poolKey];
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, _poolRoot);
                obj.name = $"{poolKey}_{i}";
                obj.SetActive(false);
                
                IPoolable poolable = obj.GetComponent<IPoolable>();
                if (poolable == null)
                {
                    Debug.LogError($"[ObjectPoolManager] Prefab for '{poolKey}' missing IPoolable component!");
                    Destroy(obj);
                    continue;
                }
                
                poolable.IsActiveInPool = false;
                pool.Push(poolable);
                _totalPooledObjects++;
            }
            
            Debug.Log($"[ObjectPoolManager] Pre-warmed '{poolKey}' pool with {count} instances");
        }
        
        /// <summary>
        /// Get an object from the pool.
        /// Zero allocation if pool has available instances.
        /// </summary>
        public T Get<T>(string poolKey, Vector3 position, Quaternion rotation) where T : class, IPoolable
        {
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found! Did you forget to RegisterPrefab?");
                return null;
            }
            
            Stack<IPoolable> pool = _pools[poolKey];
            IPoolable poolable;
            
            // Try to get from pool (zero allocation)
            if (pool.Count > 0)
            {
                poolable = pool.Pop();
                _poolHits++;
            }
            else
            {
                // Pool exhausted - create new instance (1 allocation)
                _poolMisses++;
                
                if (!_prefabs.ContainsKey(poolKey))
                {
                    Debug.LogError($"[ObjectPoolManager] No prefab registered for '{poolKey}'!");
                    return null;
                }
                
                // Check pool limit
                int activeCount = _activeObjects[poolKey].Count;
                int poolLimit = _poolLimits[poolKey];
                
                if (activeCount >= poolLimit)
                {
                    Debug.LogWarning($"[ObjectPoolManager] Pool '{poolKey}' exceeded limit ({poolLimit}). Reusing oldest object.");
                    // Force return oldest active object
                    HashSet<IPoolable>.Enumerator enumerator = _activeObjects[poolKey].GetEnumerator();
                    enumerator.MoveNext();
                    IPoolable oldest = enumerator.Current;
                    ReturnToPool(oldest);
                    poolable = pool.Pop();
                }
                else
                {
                    GameObject obj = Instantiate(_prefabs[poolKey], _poolRoot);
                    obj.name = $"{poolKey}_Dynamic_{_totalPooledObjects}";
                    poolable = obj.GetComponent<IPoolable>();
                    _totalPooledObjects++;
                }
            }
            
            // Setup pooled object
            Transform t = poolable.GameObject.transform;
            t.position = position;
            t.rotation = rotation;
            t.SetParent(null); // Remove from pool root
            
            poolable.OnSpawnFromPool();
            _activeObjects[poolKey].Add(poolable);
            _totalActiveObjects++;
            
            return poolable as T;
        }
        
        /// <summary>
        /// Convenience overload - spawn at origin with identity rotation.
        /// </summary>
        public T Get<T>(string poolKey) where T : class, IPoolable
        {
            return Get<T>(poolKey, Vector3.zero, Quaternion.identity);
        }
        
        /// <summary>
        /// Return an object to the pool.
        /// Zero allocation operation.
        /// </summary>
        public void ReturnToPool(IPoolable poolable)
        {
            if (poolable == null)
            {
                Debug.LogWarning("[ObjectPoolManager] Attempted to return null object to pool!");
                return;
            }
            
            if (!poolable.IsActiveInPool)
            {
                Debug.LogWarning($"[ObjectPoolManager] Object '{poolable.GameObject.name}' already returned to pool!");
                return;
            }
            
            string poolKey = poolable.PoolKey;
            
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPoolManager] Pool '{poolKey}' not found for return!");
                return;
            }
            
            // Reset object state
            poolable.OnReturnToPool();
            
            // Re-parent to pool root
            poolable.GameObject.transform.SetParent(_poolRoot);
            
            // Return to pool
            _pools[poolKey].Push(poolable);
            _activeObjects[poolKey].Remove(poolable);
            _totalActiveObjects--;
        }
        
        /// <summary>
        /// Clear a specific pool, destroying all instances.
        /// Use sparingly - typically only during scene transitions.
        /// </summary>
        public void ClearPool(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return;
            }
            
            // Destroy all inactive instances
            Stack<IPoolable> pool = _pools[poolKey];
            while (pool.Count > 0)
            {
                IPoolable poolable = pool.Pop();
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                }
            }
            
            // Destroy all active instances
            HashSet<IPoolable> active = _activeObjects[poolKey];
            foreach (IPoolable poolable in active)
            {
                if (poolable != null && poolable.GameObject != null)
                {
                    Destroy(poolable.GameObject);
                    _totalPooledObjects--;
                    _totalActiveObjects--;
                }
            }
            active.Clear();
            
            Debug.Log($"[ObjectPoolManager] Cleared pool '{poolKey}'");
        }
        
        /// <summary>
        /// Get pool statistics for admin UI or debugging.
        /// Zero allocation.
        /// </summary>
        public PoolStats GetPoolStats(string poolKey)
        {
            if (!_pools.ContainsKey(poolKey))
            {
                return default;
            }
            
            return new PoolStats
            {
                poolKey = poolKey,
                inactiveCount = _pools[poolKey].Count,
                activeCount = _activeObjects[poolKey].Count,
                totalCount = _pools[poolKey].Count + _activeObjects[poolKey].Count,
                maxPoolSize = _poolLimits[poolKey]
            };
        }
        
        /// <summary>
        /// Get all active objects of a specific pool type.
        /// Used by admin UI for inspection.
        /// Returns new HashSet to avoid modification issues.
        /// </summary>
        public HashSet<IPoolable> GetActiveObjects(string poolKey)
        {
            if (!_activeObjects.ContainsKey(poolKey))
            {
                return new HashSet<IPoolable>();
            }
            
            return new HashSet<IPoolable>(_activeObjects[poolKey]);
        }
        
        /// <summary>
        /// Get all registered pool keys.
        /// Used by admin UI for pool navigation.
        /// </summary>
        public List<string> GetAllPoolKeys()
        {
            return new List<string>(_pools.Keys);
        }
        
        #endregion
        
        #region Admin/Debug Methods
        
        /// <summary>
        /// Force clear all pools and reset statistics.
        /// Admin-only operation.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (string poolKey in _pools.Keys)
            {
                ClearPool(poolKey);
            }
            
            _poolHits = 0;
            _poolMisses = 0;
            
            Debug.Log("[ObjectPoolManager] All pools cleared");
        }
        
        /// <summary>
        /// Get overall pool performance metrics.
        /// </summary>
        public void GetPoolPerformanceMetrics(out int totalPooled, out int totalActive, out int hits, out int misses)
        {
            totalPooled = _totalPooledObjects;
            totalActive = _totalActiveObjects;
            hits = _poolHits;
            misses = _poolMisses;
        }
        
        #endregion
        
        #region Nested Types
        
        [Serializable]
        public class PoolConfiguration
        {
            [Tooltip("Unique identifier for this pool")]
            public string poolKey = "DefaultPool";
            
            [Tooltip("Prefab to pool")]
            public GameObject prefab;
            
            [Tooltip("Number of instances to pre-create")]
            public int initialSize = 10;
            
            [Tooltip("Maximum instances allowed in pool")]
            public int maxPoolSize = 100;
        }
        
        public struct PoolStats
        {
            public string poolKey;
            public int inactiveCount;
            public int activeCount;
            public int totalCount;
            public int maxPoolSize;
        }
        
        #endregion
    }
}