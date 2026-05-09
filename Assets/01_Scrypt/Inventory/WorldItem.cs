using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [SerializeField]
    private ItemCatalogManager itemCatalogManager;

    public string itemId;
    public string itemDisplayName;
    public int amount = 1;
    
}
