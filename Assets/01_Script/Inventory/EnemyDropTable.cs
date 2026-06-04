using UnityEngine;

public class EnemyDropTable : MonoBehaviour
{
    [System.Serializable]
    public struct DropEntry
    {
        /*
        [삭제] 기존 문자열 itemId 방식

        public string itemId;
        */

        // [변경] 드랍 아이템도 ItemData SO 직접 참조
        public ItemData itemData;

        public int amount;

        [Range(0f, 100f)]
        public float dropChance;
    }

    [SerializeField]
    private HealthPointManager hpManager;

    [SerializeField]
    private WorldItem worldItemPrefab;

    /*
    [삭제] ItemCatalogManager 조회 필요 없음

    [SerializeField]
    private ItemCatalogManager itemCatalogManager;
    */

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

        if (worldItemPrefab == null)
        {
            Debug.LogWarning("[EnemyDropTable] worldItemPrefab이 없습니다.");
            return;
        }

        for (int i = 0; i < dropEntries.Length; i++)
        {
            DropEntry dropEntry = dropEntries[i];

            if (dropEntry.itemData == null)
            {
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

            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
            Vector3 spawnPosition = transform.position + offset;

            WorldItem worldItem = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);

            // [변경] ItemData SO를 WorldItem에 직접 주입
            worldItem.SetItemData(dropEntry.itemData, Mathf.Max(1, dropEntry.amount));
        }
    }
}