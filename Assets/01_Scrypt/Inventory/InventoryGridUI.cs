using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryGridUI : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;

    [SerializeField]
    private ItemCatalogManager itemCatalogManager;

    [SerializeField]
    private RectTransform slotContainer;

    [SerializeField]
    private InventorySlotView slotPrefab;

    [SerializeField]
    private ItemUseController itemUseController;

    private int selectedSlotIndex = -1;

    private readonly List<InventorySlotView> slotViewInstances = new List<InventorySlotView>();

    private void Start()
    {
        BuildSlotViews();
        RedrawAllSlots();
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged += OnInventoryChanged;
        }

        if (slotViewInstances.Count > 0)
        {
            RedrawAllSlots();
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= OnInventoryChanged;
        }
    }

    private void OnInventoryChanged()
    {
        RedrawAllSlots();
    }

    public void RefreshDisplay()
    {
        RedrawAllSlots();
    }

    private void RedrawAllSlots()
    {
        if (inventory == null || slotViewInstances.Count == 0)
        {
            return;
        }

        IReadOnlyList<InventorySlotData> slots = inventory.InventorySlots;

        for (int viewIndex = 0; viewIndex < slotViewInstances.Count; viewIndex++)
        {
            InventorySlotData slotData =
                viewIndex < slots.Count
                    ? slots[viewIndex]
                    : new InventorySlotData { itemId = string.Empty, amount = 0 };

            // [변경] Bind 내부에서 ItemData SO를 조회함
            slotViewInstances[viewIndex].Bind(slotData, itemCatalogManager);
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

        if (inventory == null)
        {
            Debug.Log("inventory == null");
            return;
        }

        for (int childIndex = slotContainer.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(slotContainer.GetChild(childIndex).gameObject);
        }

        slotViewInstances.Clear();

        int capacity = Mathf.Max(0, inventory.SlotCapacity);

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
        else if (button == PointerEventData.InputButton.Right)
        {
            UseSlotItem(slotIndex);
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

    private void UseSlotItem(int slotIndex)
    {
        if (inventory == null || itemUseController == null)
        {
            return;
        }

        if (slotIndex < 0 || slotIndex >= inventory.InventorySlots.Count)
        {
            return;
        }

        InventorySlotData slotData = inventory.InventorySlots[slotIndex];

        if (slotData.IsEmpty)
        {
            return;
        }

        if (itemUseController.UseItem(slotData.itemId))
        {
            inventory.TryRemoveItems(slotData.itemId, 1);
        }
    }
}