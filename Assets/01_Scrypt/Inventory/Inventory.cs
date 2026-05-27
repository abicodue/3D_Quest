using System;
using System.Collections.Generic;
using UnityEngine;

public struct InventorySlotData
{
    // [유지] 인벤토리 슬롯은 저장용으로 itemId만 보관
    public string itemId;
    public int amount;

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || amount <= 0;
}

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private ItemCatalogManager itemCatalogManager;

    [SerializeField]
    private int slotCapacity = 10;

    public List<string> itemIds = new List<string>();
    public Queue<string> pickUpMessages = new Queue<string>();
    public Stack<string> undoStack = new Stack<string>();
    public Dictionary<string, int> itemCountById = new Dictionary<string, int>();

    public event Action InventoryChanged;

    private readonly List<InventorySlotData> inventorySlots = new List<InventorySlotData>();

    public int SlotCapacity => slotCapacity;
    public IReadOnlyList<InventorySlotData> InventorySlots => inventorySlots;

    private void Awake()
    {
        EnsureCatalogReference();
        InitSlots();
    }

    /*
    [삭제] ItemCatalogEntry 조회 제거

    public bool TryGetCatalogEntry(string itemId, out ItemCatalogEntry entry)
    {
        entry = default;
        return itemCatalogManager != null && itemCatalogManager.TryGetEntry(itemId, out entry);
    }
    */

    // [변경] ItemData 직접 조회
    public bool TryGetItemData(string itemId, out ItemData itemData)
    {
        itemData = null;
        return itemCatalogManager != null && itemCatalogManager.TryGetItemData(itemId, out itemData);
    }

    // [추가] SO를 직접 넘겨도 인벤토리에 추가할 수 있게 함
    public bool TryAddItems(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            return false;
        }

        return TryAddItems(itemData.Id, amount);
    }

    // [추가] SO를 직접 넘기는 획득용 함수
    public bool TryAddItemsFromPickup(ItemData itemData, int amount, out int addedAmount)
    {
        addedAmount = 0;

        if (itemData == null)
        {
            return false;
        }

        return TryAddItemsFromPickup(itemData.Id, amount, out addedAmount);
    }

    // [추가]
    public bool TryRemoveItems(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            return false;
        }

        return TryRemoveItems(itemData.Id, amount);
    }

    private void InitSlots()
    {
        inventorySlots.Clear();

        int safeCapacity = Mathf.Max(0, slotCapacity);

        for (int i = 0; i < safeCapacity; i++)
        {
            inventorySlots.Add(new InventorySlotData
            {
                itemId = string.Empty,
                amount = 0
            });
        }
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i] = new InventorySlotData
            {
                itemId = string.Empty,
                amount = 0
            };
        }
    }

    public void UndoLastPickup()
    {
        if (undoStack.Count <= 0)
        {
            Debug.Log("nothing to undo");
            return;
        }

        string lastItemId = undoStack.Pop();

        if (itemIds.Remove(lastItemId))
        {
            RemoveOneUnitFromSlotsFromEnd(lastItemId);
            DecreaseItemCount(lastItemId, 1);

            Debug.Log($"최근 획득 아이템 취소: {lastItemId}");

            PrintInventory();
            RaiseInventoryChanged();
        }
    }

    public void ProcessNextMessage()
    {
        if (pickUpMessages.Count > 0)
        {
            string message = pickUpMessages.Dequeue();
            Debug.Log(message);
        }
        else
        {
            Debug.Log("no pickUpMessages left");
        }
    }

    public void EnqueuePickUpMessage(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return;
        }

        string normalizedId = itemId.Trim();
        string displayName = ResolveDisplayName(normalizedId);

        pickUpMessages.Enqueue($"{displayName} x {amount} 획득");
        Debug.Log($"[Inventory] {displayName} x {amount} 획득");
    }

    public bool TryAddItems(string itemId, int amount)
    {
        return TryAddItemsInternal(itemId, amount, false, out _);
    }

    public bool TryAddItemsFromPickup(string itemId, int amount, out int addedAmount)
    {
        return TryAddItemsInternal(itemId, amount, true, out addedAmount);
    }

    private bool TryAddItemsInternal(string itemId, int amount, bool recordPerUnitForUndo, out int addedAmount)
    {
        addedAmount = 0;

        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return false;
        }

        itemId = itemId.Trim();

        if (!IsRegisteredItemId(itemId))
        {
            Debug.LogWarning($"[Inventory] 등록되지 않은 itemId: {itemId}");
            return false;
        }

        int roomInSlots = GetTotalRoomForItemInSlots(itemId);

        if (roomInSlots <= 0)
        {
            return false;
        }

        int toAdd = Mathf.Min(amount, roomInSlots);

        if (toAdd <= 0)
        {
            return false;
        }

        int placed = PlaceAmountIntoSlots(itemId, toAdd);

        if (placed <= 0)
        {
            return false;
        }

        if (placed != toAdd)
        {
            Debug.LogWarning($"[Inventory] placed: {placed}, expected: {toAdd}. 로직 확인 필요.");
        }

        for (int i = 0; i < placed; i++)
        {
            itemIds.Add(itemId);

            if (recordPerUnitForUndo)
            {
                undoStack.Push(itemId);
            }
        }

        IncreaseItemCount(itemId, placed);

        addedAmount = placed;

        RaiseInventoryChanged();

        return true;
    }

    public bool TryRemoveItems(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return false;
        }

        itemId = itemId.Trim();

        if (GetItemCount(itemId) < amount)
        {
            return false;
        }

        RemoveAmountFromSlots(itemId, amount);
        RemoveFromItemIdList(itemId, amount);
        DecreaseItemCount(itemId, amount);

        RaiseInventoryChanged();

        return true;
    }

    public bool HasAtLeast(string itemId, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return GetItemCount(itemId.Trim()) >= amount;
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        if (itemCountById.TryGetValue(itemId.Trim(), out int count))
        {
            return count;
        }

        return 0;
    }

    private void PrintInventory()
    {
        Debug.Log("[Inventory: List]");

        for (int i = 0; i < itemIds.Count; i++)
        {
            Debug.Log(itemIds[i]);
        }

        Debug.Log($"[Inventory Count] : {itemIds.Count}");

        Debug.Log("[Inventory: Dictionary]");

        foreach (KeyValuePair<string, int> pair in itemCountById)
        {
            Debug.Log($"{pair.Key} : {pair.Value}");
        }

        Debug.Log("[Inventory: Slots]");

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlotData slot = inventorySlots[i];
            Debug.Log($"[{i}] {(slot.IsEmpty ? "empty" : $"{slot.itemId} x {slot.amount}")}");
        }
    }

    private void RaiseInventoryChanged()
    {
        InventoryChanged?.Invoke();
    }

    private int GetTotalRoomForItemInSlots(string itemId)
    {
        int maxStack = GetMaxStackForItem(itemId);
        long room = 0;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlotData slot = inventorySlots[i];

            if (slot.IsEmpty)
            {
                room += maxStack;
            }
            else if (slot.itemId == itemId)
            {
                room += maxStack - slot.amount;
            }
        }

        if (room >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)room;
    }

    private int FindFirstStackableSlotIndex(string itemId, int maxStack)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlotData slot = inventorySlots[i];

            if (slot.IsEmpty || slot.itemId != itemId)
            {
                continue;
            }

            if (slot.amount < maxStack)
            {
                return i;
            }
        }

        return -1;
    }

    private int FindFirstEmptySlotIndex()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                return i;
            }
        }

        return -1;
    }

    private int PlaceAmountIntoSlots(string itemId, int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int maxStack = GetMaxStackForItem(itemId);
        int remaining = amount;
        int totalPlaced = 0;

        while (remaining > 0)
        {
            int slotIndex = FindFirstStackableSlotIndex(itemId, maxStack);

            if (slotIndex < 0)
            {
                slotIndex = FindFirstEmptySlotIndex();
            }

            if (slotIndex < 0)
            {
                break;
            }

            InventorySlotData slot = inventorySlots[slotIndex];

            int currentInSlot = slot.IsEmpty ? 0 : slot.amount;
            int canFit = maxStack - currentInSlot;

            if (canFit <= 0)
            {
                Debug.LogWarning("[Inventory] 슬롯 계산 불일치");
                break;
            }

            int put = Mathf.Min(remaining, canFit);

            slot.itemId = itemId;
            slot.amount = currentInSlot + put;

            inventorySlots[slotIndex] = slot;

            remaining -= put;
            totalPlaced += put;
        }

        return totalPlaced;
    }

    private void RemoveAmountFromSlots(string itemId, int amount)
    {
        int remaining = amount;

        for (int i = inventorySlots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            InventorySlotData slot = inventorySlots[i];

            if (slot.IsEmpty || slot.itemId != itemId)
            {
                continue;
            }

            int take = Math.Min(slot.amount, remaining);

            slot.amount -= take;
            remaining -= take;

            if (slot.amount <= 0)
            {
                slot.itemId = string.Empty;
                slot.amount = 0;
            }

            inventorySlots[i] = slot;
        }
    }

    private void RemoveOneUnitFromSlotsFromEnd(string itemId)
    {
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            InventorySlotData slot = inventorySlots[i];

            if (slot.IsEmpty || slot.itemId != itemId)
            {
                continue;
            }

            slot.amount--;

            if (slot.amount <= 0)
            {
                slot.itemId = string.Empty;
                slot.amount = 0;
            }

            inventorySlots[i] = slot;
            return;
        }
    }

    private bool IsRegisteredItemId(string targetId)
    {
        return itemCatalogManager != null && itemCatalogManager.IsRegistered(targetId);
    }

    private int GetMaxStackForItem(string itemId)
    {
        return itemCatalogManager != null ? itemCatalogManager.GetMaxStack(itemId) : 0;
    }

    private string ResolveDisplayName(string itemId)
    {
        return itemCatalogManager != null ? itemCatalogManager.ResolveDisplayName(itemId) : itemId;
    }

    private void IncreaseItemCount(string itemId, int amount)
    {
        if (itemCountById.TryGetValue(itemId, out int currentCount))
        {
            itemCountById[itemId] = currentCount + amount;
        }
        else
        {
            itemCountById[itemId] = amount;
        }
    }

    private void DecreaseItemCount(string itemId, int amount)
    {
        if (!itemCountById.TryGetValue(itemId, out int currentCount))
        {
            return;
        }

        int nextCount = currentCount - amount;

        if (nextCount <= 0)
        {
            itemCountById.Remove(itemId);
        }
        else
        {
            itemCountById[itemId] = nextCount;
        }
    }

    private void RemoveFromItemIdList(string itemId, int amount)
    {
        int removed = 0;

        for (int i = itemIds.Count - 1; i >= 0 && removed < amount; i--)
        {
            if (itemIds[i] == itemId)
            {
                itemIds.RemoveAt(i);
                removed++;
            }
        }
    }

    private void EnsureCatalogReference()
    {
        if (itemCatalogManager == null)
        {
            itemCatalogManager = FindFirstObjectByType<ItemCatalogManager>();
        }

        if (itemCatalogManager == null)
        {
            Debug.LogWarning("[Inventory] ItemCatalogManager 참조가 없습니다. 아이템 등록 검증이 실패할 수 있습니다.");
        }
    }
}