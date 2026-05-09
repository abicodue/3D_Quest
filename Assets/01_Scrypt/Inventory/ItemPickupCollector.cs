using UnityEngine;

public class ItemPickupCollector : MonoBehaviour
{
    [SerializeField]
    Inventory inventory;
    [SerializeField]
    ItemCatalogManager itemCatalogManager;

    private void OnTriggerEnter(Collider other)
    {
        WorldItem worldItem = other.GetComponent<WorldItem>();

        if (worldItem == null)
        {
            Debug.Log("worldItem null");
            return;
        }

        string normalizedId = NormalizeItemId(worldItem.itemId);
        if (string.IsNullOrEmpty(normalizedId))
        {
            Debug.Log("itemId null or empty");
            return;
        }

        if (!itemCatalogManager.IsRegistered(normalizedId))
        {
            Debug.Log("item not registered");
            return;
        }        

        int requestedAmount = Mathf.Max(1, worldItem.amount);

        if (!inventory.TryAddItemsFromPickup(normalizedId, requestedAmount, out int addedAmount))
        {
            Debug.Log("TryAddItemsFromPickup failed");
            return;
        }

        inventory.EnqueuePickUpMessage(normalizedId, addedAmount);
        Destroy(worldItem.gameObject);
    }

    private static string NormalizeItemId(string rawItemId)
    {
        return string.IsNullOrWhiteSpace(rawItemId) ? string.Empty : rawItemId.Trim();
    }
}
