using UnityEngine;

public class ItemPickupCollector : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private ItemCatalogManager itemCatalogManager;

    // [추가] RayCast로 감지된 현재 WorldItem을 가져오기 위한 참조
    [SerializeField]
    private PlayerInteractionDetector interactionDetector;

    // [추가] 아이템 획득 입력 키
    [SerializeField]
    private KeyCode pickupKey = KeyCode.E;

    [Header("Input Block Panels")]
    [SerializeField]
    private GameObject[] inputBlockPanels;

    // [추가] UI를 닫은 프레임에 같은 E 입력으로 아이템까지 먹어지는 것을 방지
    private bool wasInputBlockedLastFrame;

    /*
    [삭제] OnTriggerEnter 획득 방식 제거

    [SerializeField]
    private LayerMask itemLayerMask;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & itemLayerMask) == 0)
        {
            return;
        }

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
    */

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (itemCatalogManager == null)
        {
            itemCatalogManager = FindFirstObjectByType<ItemCatalogManager>();
        }

        // [추가]
        if (interactionDetector == null)
        {
            interactionDetector = GetComponent<PlayerInteractionDetector>();
        }
    }

    // [추가]
    private void Start()
    {
        wasInputBlockedLastFrame = IsInputBlocked();
    }

    // [추가] E 입력을 여기서 직접 처리
    private void Update()
    {
        if (!Input.GetKeyDown(pickupKey))
        {
            return;
        }

        if (wasInputBlockedLastFrame || IsInputBlocked())
        {
            return;
        }

        if (interactionDetector == null)
        {
            Debug.LogWarning("[Pickup] PlayerInteractionDetector 참조가 없습니다.");
            return;
        }

        WorldItem targetItem = interactionDetector.CurrentWorldItem;

        if (targetItem == null)
        {
            return;
        }

        TryPickup(targetItem);
    }

    // [추가]
    private void LateUpdate()
    {
        wasInputBlockedLastFrame = IsInputBlocked();
    }

    // [변경] RayCast로 감지한 WorldItem을 E 입력으로 획득
    public bool TryPickup(WorldItem worldItem)
    {
        if (worldItem == null)
        {
            return false;
        }

        ItemData itemData = worldItem.ItemData;

        if (itemData == null)
        {
            Debug.Log("[Pickup] WorldItem에 ItemData가 연결되어 있지 않습니다.");
            return false;
        }

        string itemId = itemData.Id;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.Log("[Pickup] ItemData.itemId가 비어 있습니다.");
            return false;
        }

        if (!itemData.canPickup)
        {
            Debug.Log($"[Pickup] {itemData.DisplayName}은/는 획득할 수 없습니다.");
            return false;
        }

        if (itemCatalogManager != null && !itemCatalogManager.IsRegistered(itemId))
        {
            Debug.LogWarning($"[Pickup] ItemCatalogManager에 등록되지 않은 아이템입니다: {itemId}");
            return false;
        }

        if (inventory == null)
        {
            Debug.LogWarning("[Pickup] Inventory 참조가 없습니다.");
            return false;
        }

        int requestedAmount = worldItem.Amount;

        // [변경] ItemData SO를 직접 넘기는 방식
        if (!inventory.TryAddItemsFromPickup(itemData, requestedAmount, out int addedAmount))
        {
            Debug.Log("[Pickup] 인벤토리에 추가하지 못했습니다.");
            return false;
        }

        inventory.EnqueuePickUpMessage(itemId, addedAmount);

        if (addedAmount >= requestedAmount)
        {
            // [변경] 씬에서 아이템 비활성화
            worldItem.gameObject.SetActive(false);
        }
        else
        {
            worldItem.SetAmount(requestedAmount - addedAmount);
        }

        Debug.Log($"[Pickup] {itemData.DisplayName} x {addedAmount} 획득");

        return true;
    }

    // [추가]
    private bool IsInputBlocked()
    {
        if (inputBlockPanels == null)
        {
            return false;
        }

        for (int i = 0; i < inputBlockPanels.Length; i++)
        {
            GameObject panel = inputBlockPanels[i];

            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }
}