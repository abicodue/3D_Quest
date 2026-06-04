using System.Collections.Generic;
using UnityEngine;

/*
[삭제] ItemCatalogEntry 체제 제거

using System;

[Serializable]
public class ItemCatalogEntry
{
    public string itemId;
    public string displayName;
    public string description;
    public ItemType category;
    public int maxStack;
    public bool canPickup = true;
    public Sprite icon;
    public Material mat;
    public Color iconTint = Color.white;

    [NonSerialized]
    public ItemData sourceData;

    public static ItemCatalogEntry FromItemData(ItemData data)
    {
        return new ItemCatalogEntry
        {
            itemId = data.itemId,
            displayName = data.itemName,
            description = data.description,
            category = data.itemType,
            maxStack = data.maxStackCount,
            canPickup = data.canPickup,
            icon = data.icon,
            mat = data.mat,
            iconTint = data.iconTint,
            sourceData = data
        };
    }
}
*/

public class ItemCatalogManager : MonoBehaviour, IItemCatalogReader
{
    [Header("ScriptableObject Item Data")]
    [SerializeField]
    private ItemData[] itemDataAssets; // [변경] ItemData SO만 등록

    /*
    [삭제] 기존 수동 Entry 배열 제거

    [SerializeField]
    private ItemCatalogEntry[] itemCatalogEntries;
    */

    // [변경] ItemCatalogEntry 딕셔너리 대신 ItemData 딕셔너리 사용
    private readonly Dictionary<string, ItemData> itemDataById = new Dictionary<string, ItemData>();

    private void Awake()
    {
        BuildCatalogDictionary();
    }

    public bool TryGetItemData(string itemId, out ItemData itemData)
    {
        itemData = null;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return itemDataById.TryGetValue(itemId.Trim(), out itemData);
    }

    public bool IsRegistered(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return itemDataById.ContainsKey(itemId.Trim());
    }

    public int GetMaxStack(string itemId)
    {
        if (!TryGetItemData(itemId, out ItemData itemData))
        {
            return 0;
        }

        return itemData.MaxStack;
    }

    public string ResolveDisplayName(string itemId)
    {
        if (TryGetItemData(itemId, out ItemData itemData))
        {
            return itemData.DisplayName;
        }

        return string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
    }

    // [추가] 아이콘이 필요한 스크립트에서 직접 쓸 수 있음
    public Sprite ResolveIcon(string itemId)
    {
        if (TryGetItemData(itemId, out ItemData itemData))
        {
            return itemData.icon;
        }

        return null;
    }

    // [추가]
    public Color ResolveIconTint(string itemId)
    {
        if (TryGetItemData(itemId, out ItemData itemData))
        {
            return itemData.IconTint;
        }

        return Color.white;
    }

    private void BuildCatalogDictionary()
    {
        itemDataById.Clear();

        if (itemDataAssets == null || itemDataAssets.Length == 0)
        {
            Debug.LogWarning("[ItemCatalogManager] 등록된 ItemData SO가 없습니다.");
            return;
        }

        for (int i = 0; i < itemDataAssets.Length; i++)
        {
            ItemData itemData = itemDataAssets[i];

            if (itemData == null)
            {
                Debug.LogWarning($"[ItemCatalogManager] itemDataAssets[{i}]가 비어 있습니다.");
                continue;
            }

            RegisterItemData(itemData, i);
        }
    }

    private void RegisterItemData(ItemData itemData, int index)
    {
        string normalizedId = itemData.Id;

        if (string.IsNullOrWhiteSpace(normalizedId))
        {
            Debug.LogWarning($"[ItemCatalogManager] itemDataAssets[{index}]의 itemId가 비어 있습니다.");
            return;
        }

        if (itemDataById.ContainsKey(normalizedId))
        {
            Debug.LogWarning($"[ItemCatalogManager] 중복 itemId 발견: {normalizedId}");
            return;
        }

        itemDataById.Add(normalizedId, itemData);
    }
}