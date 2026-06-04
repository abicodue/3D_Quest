using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopGridUI : MonoBehaviour
{
    [SerializeField]
    private Inventory playerInventory;

    [SerializeField]
    private ItemCatalogManager itemCatalogManager;

    [SerializeField]
    private RectTransform slotContainer;

    [SerializeField]
    private InventorySlotView slotPrefab;

    private Shop shop;

    private int selectedSlotIndex = -1;

    private readonly List<InventorySlotView> slotViewInstances = new List<InventorySlotView>();

    private void Start()
    {
        // [변경] shop이 연결된 뒤 SetShop에서 슬롯을 만들기 때문에 여기서는 null일 수 있음
        if (shop != null)
        {
            BuildSlotViews();
            RedrawAllSlots();
        }
    }

    public void RefreshDisplay()
    {
        RedrawAllSlots();
    }

    private void RedrawAllSlots()
    {
        if (shop == null || slotViewInstances.Count == 0)
        {
            return;
        }

        IReadOnlyList<ShopSlotData> slots = shop.ShopSlots;

        for (int viewIndex = 0; viewIndex < slotViewInstances.Count; viewIndex++)
        {
            ShopSlotData slotData =
                viewIndex < slots.Count
                    ? slots[viewIndex]
                    : new ShopSlotData { itemData = null, price = 0 };

            InventorySlotData displaySlot =
                slotData.IsEmpty
                    ? new InventorySlotData { itemId = string.Empty, amount = 0 }
                    : new InventorySlotData { itemId = slotData.ItemId, amount = slotData.Price };

            // [변경] ItemData SO 기반 Catalog에서 UI 아이콘 조회
            slotViewInstances[viewIndex].Bind(displaySlot, itemCatalogManager);
            slotViewInstances[viewIndex].SetSelected(viewIndex == selectedSlotIndex);
        }
    }

    private void BuildSlotViews()
    {
        if (slotContainer == null || slotPrefab == null)
        {
            Debug.Log("slotContainer == null || slotPrefab == null");
            return;
        }

        if (shop == null)
        {
            Debug.Log("shop == null");
            return;
        }

        for (int childIndex = slotContainer.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(slotContainer.GetChild(childIndex).gameObject);
        }

        slotViewInstances.Clear();

        int capacity = Mathf.Max(0, shop.SlotCapacity);

        for (int slotIndex = 0; slotIndex < capacity; slotIndex++)
        {
            InventorySlotView slotInstance = Instantiate(slotPrefab, slotContainer);
            slotInstance.gameObject.name = $"ShopSlot_{slotIndex:D2}";

            int capturedIndex = slotIndex;
            slotInstance.OnClicked += (slotView, button) => OnSlotClicked(capturedIndex, button);

            slotViewInstances.Add(slotInstance);
        }
    }

    private void OnSlotClicked(int slotIndex, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
        {
            SelectSlot(slotIndex);
        }
    }

    private void SelectSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;

        for (int i = 0; i < slotViewInstances.Count; i++)
        {
            slotViewInstances[i].SetSelected(i == selectedSlotIndex);
        }
    }

    private void BuySlotItem(int slotIndex)
    {
        if (shop == null || playerInventory == null)
        {
            return;
        }

        if (slotIndex < 0 || slotIndex >= shop.ShopSlots.Count)
        {
            return;
        }

        ShopSlotData slotData = shop.ShopSlots[slotIndex];

        if (slotData.IsEmpty)
        {
            return;
        }

        string currencyItemId = shop.CurrencyItemId;
        int price = slotData.Price;

        if (!playerInventory.HasAtLeast(currencyItemId, price))
        {
            Debug.Log("돈이 충분하지 않습니다.");
            return;
        }

        /*
        [삭제] 기존 string itemId 구매 방식

        if (!playerInventory.TryAddItems(slotData.itemId, 1))
        {
            Debug.Log("인벤토리에 공간이 충분하지 않습니다.");
            return;
        }
        */

        // [변경] ItemData SO를 직접 넘겨 인벤토리에 추가
        if (!playerInventory.TryAddItems(slotData.ItemData, 1))
        {
            Debug.Log("인벤토리에 공간이 충분하지 않습니다.");
            return;
        }

        playerInventory.TryRemoveItems(currencyItemId, price);
        shop.EnqueueBuyMessage(slotData.ItemData, price);
    }

    public void BuySelectedItem()
    {
        if (selectedSlotIndex < 0)
        {
            return;
        }

        BuySlotItem(selectedSlotIndex);
    }

    public void UndoLastBuy()
    {
        if (shop == null || playerInventory == null)
        {
            return;
        }

        shop.UndoLastBuy();
    }

    public void SetShop(Shop targetShop)
    {
        shop = targetShop;
        selectedSlotIndex = -1;

        BuildSlotViews();
        RedrawAllSlots();
    }
}