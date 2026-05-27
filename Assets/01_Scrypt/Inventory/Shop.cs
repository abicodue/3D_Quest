using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ShopSlotData
{
    /*
    [삭제] 기존 문자열 itemId 방식

    public string itemId;
    public int price;
    public bool IsEmpty => string.IsNullOrEmpty(itemId) || price <= 0;
    */

    // [변경] 상점 상품도 ItemData SO 직접 참조
    public ItemData itemData;

    // [유지] 0 이하로 두면 ItemData.buyPrice 사용
    public int price;

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

    public int Price
    {
        get
        {
            if (price > 0)
            {
                return price;
            }

            return itemData != null ? itemData.BuyPrice : 0;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return itemData == null || string.IsNullOrWhiteSpace(ItemId) || Price <= 0;
        }
    }
}

public struct BuyRecord
{
    /*
    [삭제] 기존 문자열 저장 방식

    public string itemId;
    public int price;
    */

    // [변경]
    public ItemData itemData;
    public int price;

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
}

public class Shop : MonoBehaviour
{
    /*
    [삭제] Shop 자체에서는 Catalog가 필요 없어짐.
    상점 슬롯이 ItemData를 직접 들고 있음.

    [SerializeField]
    private ItemCatalogManager itemCatalogManager;
    */

    [SerializeField]
    private Inventory playerInventory;

    [SerializeField]
    private int slotCapacity = 10;

    [SerializeField]
    private ShopSlotData[] initialShopSlots;

    [Header("Currency")]
    [SerializeField]
    private string currencyItemId = "item_coin";

    public Queue<string> buyMessages = new Queue<string>();
    public Stack<BuyRecord> undoStack = new Stack<BuyRecord>();

    private readonly List<ShopSlotData> shopSlots = new List<ShopSlotData>();

    public int SlotCapacity => slotCapacity;
    public IReadOnlyList<ShopSlotData> ShopSlots => shopSlots;
    public string CurrencyItemId => currencyItemId;

    private void Awake()
    {
        EnsureInventoryReference();
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
                shopSlots.Add(new ShopSlotData
                {
                    itemData = null,
                    price = 0
                });
            }
        }
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < shopSlots.Count; i++)
        {
            shopSlots[i] = new ShopSlotData
            {
                itemData = null,
                price = 0
            };
        }
    }

    public void ProcessNextMessage()
    {
        if (buyMessages.Count > 0)
        {
            string message = buyMessages.Dequeue();
            Debug.Log(message);
        }
        else
        {
            Debug.Log("no buyMessages left");
        }
    }

    // [변경] string itemId 대신 ItemData를 받음
    public void EnqueueBuyMessage(ItemData itemData, int price)
    {
        if (itemData == null || price <= 0)
        {
            return;
        }

        undoStack.Push(new BuyRecord
        {
            itemData = itemData,
            price = price
        });

        buyMessages.Enqueue($"{itemData.DisplayName} 구매 / 가격: {price}");
        Debug.Log($"[Shop] {itemData.DisplayName} 구매 / 가격: {price}");
    }

    private void EnsureInventoryReference()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<Inventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("[Shop] Player Inventory 참조가 없습니다.");
        }
    }

    public void UndoLastBuy()
    {
        if (playerInventory == null)
        {
            return;
        }

        if (undoStack.Count <= 0)
        {
            Debug.Log("Nothing to Undo");
            return;
        }

        BuyRecord lastBuy = undoStack.Pop();

        if (lastBuy.itemData == null)
        {
            return;
        }

        if (playerInventory.TryRemoveItems(lastBuy.itemData, 1))
        {
            playerInventory.TryAddItems(currencyItemId, lastBuy.price);
            Debug.Log($"[Shop] {lastBuy.DisplayName} 환불 / {lastBuy.price} {currencyItemId} 반환");
        }
        else
        {
            undoStack.Push(lastBuy);
            Debug.Log($"[Shop] 환불 실패: {lastBuy.DisplayName}");
        }
    }
}