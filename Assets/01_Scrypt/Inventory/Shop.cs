using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ShopSlotData
{
    public string itemId;
    public int price;

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || price <= 0;

}

public struct BuyRecord
{
    public string itemId;
    public int price;
}

public class Shop : MonoBehaviour
{
    [SerializeField]
    private ItemCatalogManager itemCatalogManager;
    [SerializeField]
    private Inventory playerInventory;
    [SerializeField]
    private int slotCapacity = 10;
    [SerializeField]
    private ShopSlotData[] initialShopSlots;

    public Queue<string> buyMessages = new Queue<string>();
    public Stack<BuyRecord> undoStack = new Stack<BuyRecord>();

    private readonly List<ShopSlotData> shopSlots = new List<ShopSlotData>();

    public int SlotCapacity => slotCapacity;
    public IReadOnlyList<ShopSlotData> ShopSlots => shopSlots;

    private void Awake()
    {
        EnsureCatalogReference();
        InitSlots();
    }

    public bool TryGetCatalogEntry(string itemId, out ItemCatalogEntry entry)
    {
        entry = default;
        return itemCatalogManager != null && itemCatalogManager.TryGetEntry(itemId, out entry);
    }

    private void InitSlots()
    {
        shopSlots.Clear();
        
        int safeCapacity = Mathf.Max(0, slotCapacity);
        
        for (int i = 0; i < safeCapacity; i++)
        {
            if (initialShopSlots != null && i < initialShopSlots.Length)
            {
                shopSlots.Add(initialShopSlots[i]);
            }
            else
            {
                shopSlots.Add(new ShopSlotData { itemId = string.Empty, price = 0 });
            }
            
           
        }
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < shopSlots.Count; i++)
        {
            shopSlots[i] = new ShopSlotData { itemId = string.Empty, price = 0 };
        }
    }    

    public void ProcessNextMessage()
    {
        if (buyMessages.Count > 0)
        {
            string message = buyMessages.Dequeue();
            Debug.Log($"{message}");
        }
        else
        {
            Debug.Log("no buyMessages left");
        }
    }

    public void EnqueueBuyMessage(string itemId, int price)
    {
        if (string.IsNullOrWhiteSpace(itemId) || price <= 0)
        {
            return;
        }

        string normalizedId = itemId.Trim();
        string displayName = ResolveDisplayName(normalizedId);

        undoStack.Push(new BuyRecord
        {
            itemId = normalizedId,
            price = price
        });

        buyMessages.Enqueue($"{displayName} 구매 / 가격: {price}");
        Debug.Log($"[Shop] {displayName} 구매 / 가격: {price}");
    }      

    private string ResolveDisplayName(string itemId)
    {
        return itemCatalogManager != null ? itemCatalogManager.ResolveDisplayName(itemId) : itemId;
    }    

    private void EnsureCatalogReference()
    {
        if (itemCatalogManager == null)
        {
            itemCatalogManager = FindFirstObjectByType<ItemCatalogManager>();
        }

        if (itemCatalogManager == null)
        {
            Debug.LogWarning("[Shop] ItemCatalogManager 참조가 없습니다. 카탈로그 기반 검증이 실패할 수 있습니다.");
        }
    }

    public void UndoLastBuy()
    {
        if (playerInventory == null )
        {
            return;
        }

        if (undoStack.Count <= 0)
        {
            Debug.Log("Nothing to Undo");
            return;
        }

        BuyRecord lastBuy = undoStack.Pop();
        string displayName = ResolveDisplayName(lastBuy.itemId);

        if (playerInventory.TryRemoveItems(lastBuy.itemId, 1))
        {
            playerInventory.TryAddItems("coin", lastBuy.price);
            Debug.Log($"[Shop] {displayName} 환불 / {lastBuy.price} coin 반환");
        }
        else
        {
            undoStack.Push(lastBuy);
            Debug.Log($"[Shop] 환불 실패: {displayName}");
        }
    }

}
