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
        BuildSlotViews();
        RedrawAllSlots();
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
            ShopSlotData slotData = viewIndex < slots.Count ? slots[viewIndex] : new ShopSlotData { itemId = string.Empty, price = 0 };

            InventorySlotData displaySlot = slotData.IsEmpty ? new InventorySlotData { itemId = string.Empty, amount = 0 } : new InventorySlotData { itemId = slotData.itemId, amount = slotData.price };

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
            slotInstance.gameObject.name = $"Slot_{slotIndex:D2}";

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
        if (shop == null || playerInventory == null || slotIndex < 0 || slotIndex >= shop.ShopSlots.Count)
        {
            return;
        }

        ShopSlotData slotData = shop.ShopSlots[slotIndex];

        if (slotData.IsEmpty)
        {
            return;
        }

        if (!playerInventory.HasAtLeast("coin", slotData.price))
        {
            Debug.Log($"돈이 충분하지 않습니다.");
            return;
        }

        if (!playerInventory.TryAddItems(slotData.itemId, 1))
        {
            Debug.Log($"인벤토리에 공간이 충분하지 않습니다.");
            return;
        }        

        playerInventory.TryRemoveItems("coin", slotData.price);
        shop.EnqueueBuyMessage(slotData.itemId, slotData.price);
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
