using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Systems;

namespace Game.Core
{
    /// <summary>
    /// Application entry point and bootstrap system.
    /// Ensures CoreSystemManager exists and handles initial scene loading.
    /// Place this on a GameObject in your first scene (typically "Bootstrap" scene).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField, Tooltip("Scene to load after initialization")]
        private string _initialSceneName = "MainMenu";
        
        [SerializeField, Tooltip("Show loading screen during initialization")]
        private bool _showLoadingScreen = true;
        
        [SerializeField, Tooltip("Minimum time to show loading screen (prevents flicker)")]
        private float _minLoadingTime = 1f;
        
        [Header("Performance Settings")]
        [SerializeField, Tooltip("Target frame rate (0 = unlimited)")]
        private int _targetFrameRate = 60;
        
        [SerializeField, Tooltip("Enable VSync (overrides target frame rate)")]
        private bool _enableVSync = true;
        
        [Header("Quality Settings")]
        [SerializeField, Tooltip("Default quality level on startup")]
        private int _defaultQualityLevel = 2; // Medium
        
        [Header("Runtime References (Auto-Assigned)")]
        [SerializeField] private CoreSystemManager _coreSystemManager;
        [SerializeField] private Canvas _loadingCanvas;
        
        private bool _isBootstrapped;
        
        #region Unity Lifecycle
        
        private async void Start()
        {
            if (_isBootstrapped)
            {
                Debug.LogWarning("[GameBootstrap] Already bootstrapped!");
                return;
            }
            
            await BootstrapGame();
        }
        
        private void OnApplicationQuit()
        {
            if (_coreSystemManager != null)
            {
                // Note: Cannot await in OnApplicationQuit, but we can start the shutdown
                _ = _coreSystemManager.ShutdownAllSystems();
            }
        }
        
        #endregion
        
        #region Bootstrap Process
        
        private async Awaitable BootstrapGame()
        {
            float startTime = Time.realtimeSinceStartup;
            
            Debug.Log("[GameBootstrap] Starting game bootstrap...");
            
            // Step 1: Apply performance settings
            ApplyPerformanceSettings();
            
            // Step 2: Show loading screen
            if (_showLoadingScreen)
            {
                ShowLoadingScreen();
            }
            
            // Step 3: Ensure CoreSystemManager exists
            await EnsureCoreSystemManager();
            
            // Step 4: Wait for minimum loading time (prevents flicker)
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadingTime)
            {
                float waitTime = _minLoadingTime - elapsed;
                await Awaitable.WaitForSecondsAsync(waitTime);
            }
            
            // Step 5: Load initial scene
            await LoadInitialScene();
            
            // Step 6: Hide loading screen
            if (_showLoadingScreen)
            {
                HideLoadingScreen();
            }
            
            _isBootstrapped = true;
            
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[GameBootstrap] Bootstrap complete in {totalTime:F3}s");
        }
        
        private void ApplyPerformanceSettings()
        {
            // Set target frame rate
            if (_enableVSync)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1; // VSync controls framerate
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = _targetFrameRate;
            }
            
            // Set quality level
            QualitySettings.SetQualityLevel(_defaultQualityLevel, true);
            
            // Prevent screen dimming on mobile
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Debug.Log($"[GameBootstrap] Performance settings applied: " +
                     $"TargetFPS={Application.targetFrameRate}, " +
                     $"VSync={QualitySettings.vSyncCount}, " +
                     $"Quality={QualitySettings.names[_defaultQualityLevel]}");
        }
        
        private async Awaitable EnsureCoreSystemManager()
        {
            // Check if CoreSystemManager already exists
            _coreSystemManager = FindFirstObjectByType<CoreSystemManager>();
            
            if (_coreSystemManager == null)
            {
                Debug.Log("[GameBootstrap] Creating CoreSystemManager...");
                
                GameObject coreSystemObj = new("CoreSystemManager");
                _coreSystemManager = coreSystemObj.AddComponent<CoreSystemManager>();
                
                // Wait for initialization
                while (!_coreSystemManager.IsReady())
                {
                    await Awaitable.NextFrameAsync();
                }
            }
            else
            {
                Debug.Log("[GameBootstrap] CoreSystemManager already exists");
            }
        }
        
        private async Awaitable LoadInitialScene()
        {
            if (string.IsNullOrEmpty(_initialSceneName))
            {
                Debug.LogWarning("[GameBootstrap] No initial scene specified!");
                return;
            }
            
            // Check if we're already in the target scene
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == _initialSceneName)
            {
                Debug.Log($"[GameBootstrap] Already in scene '{_initialSceneName}'");
                return;
            }
            
            Debug.Log($"[GameBootstrap] Loading scene '{_initialSceneName}'...");
            
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(_initialSceneName, LoadSceneMode.Single);
            
            if (loadOp == null)
            {
                Debug.LogError($"[GameBootstrap] Failed to load scene '{_initialSceneName}'!");
                return;
            }
            
            // Wait for scene to load
            while (!loadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }
            
            Debug.Log($"[GameBootstrap] Scene '{_initialSceneName}' loaded");
        }
        
        #endregion
        
        #region Loading Screen
        
        private void ShowLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                _loadingCanvas.gameObject.SetActive(true);
                return;
            }
            
            // Create simple loading canvas if not assigned
            GameObject canvasObj = new("LoadingCanvas");
            _loadingCanvas = canvasObj.AddComponent<Canvas>();
            _loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _loadingCanvas.sortingOrder = 9999; // Top-most
            
            // Add CanvasScaler for resolution independence
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster (required for UI)
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create loading panel (simple black background)
            GameObject panelObj = new("LoadingPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
            panel.color = Color.black;
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            // Create loading text
            GameObject textObj = new("LoadingText");
            textObj.transform.SetParent(panelObj.transform, false);
            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = "Loading...";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(400, 100);
            
            DontDestroyOnLoad(canvasObj);
        }
        
        private void HideLoadingScreen()
        {
            if (_loadingCanvas != null)
            {
                Destroy(_loadingCanvas.gameObject);
                _loadingCanvas = null;
            }
        }
        
        #endregion
    }
}