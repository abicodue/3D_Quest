using UnityEngine;

public class EnemyDropTable : MonoBehaviour
{
    [System.Serializable]
    public struct DropEntry
    {
        public string itemId;
        public int amount;
        [Range(0f, 100f)]
        public float dropChance;
    }

    [SerializeField]
    private HealthPointManager hpManager;
    [SerializeField]
    private WorldItem worldItemPrefab;
    [SerializeField]
    private ItemCatalogManager itemCatalogManager;
    [SerializeField]
    private DropEntry[] dropEntries;

    private bool hasDropped = false;

    private void OnEnable()
    {
        if (hpManager != null)
        {
            hpManager.OnDied += DropItems;
        }
    }

    private void OnDisable()
    {
        if (hpManager != null)
        {
            hpManager.OnDied -= DropItems;
        }
    }

    private void DropItems()
    {
        if (hasDropped)
        {
            return;
        }

        hasDropped = true;

        if (dropEntries == null || dropEntries.Length == 0)
        {
            return;
        }

        for (int i = 0; i < dropEntries.Length; i++)
        {
            DropEntry dropEntry = dropEntries[i];

            if (string.IsNullOrWhiteSpace(dropEntry.itemId))
            {
                continue;
            }

            if (!itemCatalogManager.TryGetEntry(dropEntry.itemId, out ItemCatalogEntry entry))
            {
                Debug.Log($" {dropEntry.itemId} not in the Catalog");
                continue;
            }

            float roll = Random.Range(0f, 100f);

            if (roll > dropEntry.dropChance)
            {
                continue;
            }

            int count = dropEntries.Length;
            float angle = 360f / count * i;
            float rad = angle * Mathf.Deg2Rad;
            float radius = 1f;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            Vector3 spawnPosition = transform.position + offset;

            WorldItem worldItem = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);
            worldItem.itemId = dropEntry.itemId;
            worldItem.amount = Mathf.Max(1, dropEntry.amount);
            worldItem.itemDisplayName = entry.displayName;
            worldItem.SetMeshRenderer();

        }

    }

}
