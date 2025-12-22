// LootDrop.cs
// CREATE THIS EIGHTH - Requires ItemSystem.cs, InventoryManager.cs
// Handles pickupable loot in the world

using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    
    private Item item;
    private Vector3 startPosition;
    private float bobTimer;
    private GameObject player;
    private bool canPickup = true;
    private bool playerInRange = false;
    
    void Start()
    {
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player");
        
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = pickupRange;
    }
    
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        bobTimer += Time.deltaTime * bobSpeed;
        Vector3 newPos = startPosition;
        newPos.y += Mathf.Sin(bobTimer) * bobHeight;
        transform.position = newPos;
        
        if (canPickup && playerInRange && item != null)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire3"))
            {
                PickupItem();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canPickup)
        {
            playerInRange = true;
            //Debug.Log($"Press F to pick up {item?.itemName ?? "item"}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    void PickupItem()
    {
        if (!canPickup || item == null || player == null) return;
        
        InventoryManager inventory = player.GetComponent<InventoryManager>();
        
        if (inventory != null)
        {
            if (inventory.AddItem(item))
            {
                canPickup = false;
                //Debug.Log($"Picked up {item.rarity} {item.itemName}!");
                Destroy(gameObject);
            }
        }
    }
    
    public void SetItem(Item newItem)
    {
        item = newItem;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}