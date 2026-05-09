using UnityEngine;

public class ItemUseController : MonoBehaviour
{
    [SerializeField]
    HealthPointManager hpManager;
    [SerializeField]
    ItemCatalogManager catalog;

    public bool UseItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        string displayName = itemId;

        if (catalog != null && catalog.TryGetEntry(itemId, out ItemCatalogEntry entry))
        {
            displayName = entry.displayName;
        }
        
        if (itemId == "hp_potion")
        {
            if (hpManager != null)
            {
                Debug.Log($"{displayName}를 사용했습니다");
                hpManager.Heal(10);
                return true;
            }                
        }

        Debug.Log("사용할 수 없는 아이템입니다.");
        return false;        
        
    }

}
