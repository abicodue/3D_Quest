using UnityEngine;

public class WorldItem : MonoBehaviour
{
    /*
    [삭제] 기존 Catalog + 문자열 데이터 방식 제거

    [SerializeField]
    private ItemCatalogManager itemCatalogManager;

    public string itemId;
    public string itemDisplayName;
    */

    [Header("Item Data")]
    [SerializeField]
    private ItemData itemData; // [변경] 월드 아이템이 ItemData SO를 직접 참조

    [SerializeField]
    private int amount = 1;

    private MeshRenderer meshRenderer;

    public ItemData ItemData => itemData;

    public string ItemId
    {
        get
        {
            return itemData != null ? itemData.Id : string.Empty;
        }
    }

    public string DisplayName
    {
        get
        {
            return itemData != null ? itemData.DisplayName : string.Empty;
        }
    }

    public string Description
    {
        get
        {
            return itemData != null ? itemData.description : string.Empty;
        }
    }

    public bool CanPickup
    {
        get
        {
            return itemData != null && itemData.canPickup;
        }
    }

    public int Amount => Mathf.Max(1, amount);

    private void Awake()
    {
        CacheRenderer();
        ApplyItemVisual();
    }

    private void OnValidate()
    {
        amount = Mathf.Max(1, amount);

        CacheRenderer();
        ApplyItemVisual();
    }

    // [추가] 드랍 아이템 생성 시 SO와 수량을 주입하기 위한 함수
    public void SetItemData(ItemData data, int newAmount)
    {
        itemData = data;
        amount = Mathf.Max(1, newAmount);

        ApplyItemVisual();
    }

    // [추가] 인벤토리 공간 부족으로 일부만 획득했을 때 남은 수량 갱신
    public void SetAmount(int newAmount)
    {
        amount = Mathf.Max(1, newAmount);
    }

    /*
    [삭제] 기존 Catalog 조회 방식

    public void SetMeshRenderer()
    {
        if (itemCatalogManager.TryGetEntry(itemId, out ItemCatalogEntry entry))
        {
            meshRenderer.material = entry.mat;
        }
    }
    */

    // [변경] ItemData.mat을 직접 사용
    public void ApplyItemVisual()
    {
        if (meshRenderer == null || itemData == null || itemData.mat == null)
        {
            return;
        }

        meshRenderer.sharedMaterial = itemData.mat;
    }

    private void CacheRenderer()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }
    }
}