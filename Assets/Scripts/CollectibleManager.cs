// CollectibleManager.cs
// CREATE THIS TENTH - Requires ItemSystem.cs
// Tracks which collectibles the player has found

using UnityEngine;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    private HashSet<int> collectedIDs = new HashSet<int>();
    
    public void CollectItem(Collectible collectible)
    {
        if (!collectedIDs.Contains(collectible.collectibleID))
        {
            collectedIDs.Add(collectible.collectibleID);
            //Debug.Log($"Collected: {collectible.itemName} ({collectedIDs.Count} total collectibles)");
        }
    }
    
    public bool HasCollected(int collectibleID)
    {
        return collectedIDs.Contains(collectibleID);
    }
    
    public int GetTotalCollected()
    {
        return collectedIDs.Count;
    }
    
    public void ClearAllCollectibles()
    {
        collectedIDs.Clear();
        //Debug.Log("All collectibles cleared!");
    }
}