using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [SerializeField]
    private ItemCatalogManager itemCatalogManager;
    
    private MeshRenderer meshRenderer;

    public string itemId;
    public string itemDisplayName;
    public int amount = 1;

    private void Awake()
    {
        if (itemCatalogManager == null)
        {
            itemCatalogManager = FindFirstObjectByType<ItemCatalogManager>();
        }

        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetMeshRenderer()
    {
        if (itemCatalogManager.TryGetEntry(itemId, out ItemCatalogEntry entry))
        {
            meshRenderer.material = entry.mat;
        }
    }
    
}
