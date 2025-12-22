// CollectibleObject.cs
// CREATE THIS NINTH - Requires ItemSystem.cs, CollectibleManager.cs
// Handles collectible items scattered in the world

using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [SerializeField] private Collectible collectible;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private ParticleSystem collectEffect;
    
    private Vector3 startPosition;
    private float bobTimer;
    private bool collected = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (collectible == null)
        {
            GenerateCollectible();
        }
        
        SphereCollider collider = gameObject.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.isTrigger = true;
        collider.radius = 1.5f;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.cyan * 0.5f);
        }
    }
    
    void Update()
    {
        if (collected) return;
        
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        bobTimer += Time.deltaTime * bobSpeed;
        Vector3 newPos = startPosition;
        newPos.y += Mathf.Sin(bobTimer) * bobHeight;
        transform.position = newPos;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        
        if (other.CompareTag("Player"))
        {
            CollectibleManager manager = other.GetComponent<CollectibleManager>();
            if (manager != null)
            {
                manager.CollectItem(collectible);
                collected = true;
                
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }
                
                //Debug.Log($"Collected: {collectible.itemName}");
                Destroy(gameObject);
            }
        }
    }
    
    void GenerateCollectible()
    {
        makeItems generator = FindFirstObjectByType<makeItems>();
        if (generator != null)
        {
            collectible = generator.GenerateCollectible();
        }
        else
        {
            collectible = new Collectible();
            collectible.itemID = System.Guid.NewGuid().ToString();
            collectible.collectibleID = Random.Range(1, 1000);
            collectible.itemName = "Mystery Collectible";
            collectible.description = "A rare collectible item";
            collectible.rarity = ItemRarity.Rare;
        }
    }
}