using UnityEngine;
using System.Collections.Generic;

namespace Game.Core.Systems
{
    /// <summary>
    /// Registry for all interactable objects in zones.
    /// Manages doors, chests, ladders, stairs, windows, etc.
    /// </summary>
    public class InteractableRegistry : MonoBehaviour
    {
        [Header("Interactable Prefabs")]
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _chestPrefab;
        [SerializeField] private GameObject _ladderPrefab;
        [SerializeField] private GameObject _stairsPrefab;
        
        [Header("Runtime Registry")]
        [SerializeField] private List<InteractableObject> _registeredInteractables = new();
        
        private Dictionary<string, InteractableObject> _interactableMap;
        
        private void Awake()
        {
            _interactableMap = new Dictionary<string, InteractableObject>();
        }
        
        public void RegisterInteractable(InteractableObject interactable)
        {
            if (!_registeredInteractables.Contains(interactable))
            {
                _registeredInteractables.Add(interactable);
                _interactableMap[interactable.GetInstanceID().ToString()] = interactable;
            }
        }
        
        public void UnregisterInteractable(InteractableObject interactable)
        {
            _registeredInteractables.Remove(interactable);
            _interactableMap.Remove(interactable.GetInstanceID().ToString());
        }
        
        public List<InteractableObject> GetInteractablesInRange(Vector3 position, float range)
        {
            List<InteractableObject> nearby = new();
            
            foreach (InteractableObject interactable in _registeredInteractables)
            {
                if (interactable != null && Vector3.Distance(position, interactable.transform.position) <= range)
                {
                    nearby.Add(interactable);
                }
            }
            
            return nearby;
        }
    }
    
    /// <summary>
    /// Base class for all interactable objects.
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string _interactionPrompt = "Press E to interact";
        [SerializeField] protected float _interactionRange = 3f;
        [SerializeField] protected bool _isInteractable = true;
        
        public string InteractionPrompt => _interactionPrompt;
        public float InteractionRange => _interactionRange;
        public bool IsInteractable => _isInteractable;
        
        protected virtual void Start()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.RegisterInteractable(this);
        }
        
        protected virtual void OnDestroy()
        {
            InteractableRegistry registry = FindFirstObjectByType<InteractableRegistry>();
            registry?.UnregisterInteractable(this);
        }
        
        public abstract void Interact(GameObject interactor);
        
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = _isInteractable ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
    
    /// <summary>
    /// Door interactable - can be opened/closed, locked/unlocked.
    /// </summary>
    public class Door : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _isOpen;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _openSpeed = 2f;
        
        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isAnimating;
        
        protected override void Start()
        {
            base.Start();
            _closedRotation = transform.rotation;
            _openRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
        }
        
        public override void Interact(GameObject interactor)
        {
            if (_isLocked)
            {
                Debug.Log("[Door] Door is locked!");
                return;
            }
            
            if (!_isAnimating)
            {
                _isOpen = !_isOpen;
                StartCoroutine(AnimateDoor());
            }
        }
        
        private System.Collections.IEnumerator AnimateDoor()
        {
            _isAnimating = true;
            Quaternion targetRotation = _isOpen ? _openRotation : _closedRotation;
            
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _openSpeed);
                yield return null;
            }
            
            transform.rotation = targetRotation;
            _isAnimating = false;
        }
    }
    
    /// <summary>
    /// Chest interactable - contains loot.
    /// </summary>
    public class Chest : InteractableObject
    {
        [Header("Chest Settings")]
        [SerializeField] private bool _isOpened;
        [SerializeField] private int _lootCount = 3;
        
        public override void Interact(GameObject interactor)
        {
            if (_isOpened)
            {
                Debug.Log("[Chest] Already looted!");
                return;
            }
            
            _isOpened = true;
            SpawnLoot();
        }
        
        private void SpawnLoot()
        {
            // TODO: Integrate with LootManager
            Debug.Log($"[Chest] Spawning {_lootCount} items of loot!");
        }
    }
    
    /// <summary>
    /// Ladder interactable - allows vertical traversal.
    /// </summary>
    public class Ladder : InteractableObject
    {
        [Header("Ladder Settings")]
        [SerializeField] private float _climbSpeed = 3f;
        [SerializeField] private Transform _topPosition;
        [SerializeField] private Transform _bottomPosition;
        
        public override void Interact(GameObject interactor)
        {
            // TODO: Implement climbing mechanics
            Debug.Log("[Ladder] Climbing!");
        }
    }
}