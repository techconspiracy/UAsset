using UnityEngine;

namespace Game.Core.Systems
{
    /// <summary>
    /// WebSocket-based LAN multiplayer manager (max 6 players).
    /// Stub implementation - will be fully implemented in Phase 5.
    /// </summary>
    public partial class WebSocketNetworkManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private int _maxPlayers = 6;
        [SerializeField] private int _port = 7777;
        
        [Header("Network State")]
        [SerializeField] private bool _isHost;
        [SerializeField] private bool _isConnected;
        [SerializeField] private int _connectedPlayerCount;
        
        public bool IsHost() => _isHost;
        public bool IsConnected() => _isConnected;
        public int GetPlayerCount() => _connectedPlayerCount;
        
        public async Awaitable DisconnectAsync()
        {
            _isConnected = false;
            _isHost = false;
            _connectedPlayerCount = 0;
            await Awaitable.NextFrameAsync();
        }
    }
}