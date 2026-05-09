using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    Key,
    Quest,
    Consumable,
    Gold,
}

[Serializable]
public class ItemCatalogEntry
{
    public string itemId;
    public string displayName;
    public ItemType category;
    public int maxStack;
    public Sprite icon;
    public Color iconTint = Color.white;
}

public class ItemCatalogManager : MonoBehaviour, IItemCatalogReader
{
    [SerializeField]
    private ItemCatalogEntry[] itemCatalogEntries;

    private readonly Dictionary<string, ItemCatalogEntry> catalogById = new Dictionary<string, ItemCatalogEntry>();

    private void Awake()
    {
        EnsureCatalogNotEmptyForRuntime();
        BuildCatalogDictionary();
    }
    public int GetMaxStack(string itemId)
    {
        if (!TryGetEntry(itemId, out ItemCatalogEntry entry))
        {
            return 0;
        }

        return entry.maxStack <= 0 ? int.MaxValue : entry.maxStack;
    }

    public bool IsRegistered(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return catalogById.ContainsKey(itemId.Trim());
    }

    public string ResolveDisplayName(string itemId)
    {
        if (TryGetEntry(itemId, out ItemCatalogEntry entry) && !string.IsNullOrEmpty(entry.displayName))
        {
            return entry.displayName;
        }

        return string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
    }

    public bool TryGetEntry(string itemId, out ItemCatalogEntry entry)
    {
        entry = default;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        return catalogById.TryGetValue(itemId.Trim(), out entry);
    }

    private void EnsureCatalogNotEmptyForRuntime()
    {
        if (itemCatalogEntries != null && itemCatalogEntries.Length > 0)
        {
            return;
        }

        itemCatalogEntries = new[]
        {
            new ItemCatalogEntry { category = ItemType.Consumable, displayName = "HP Potion", icon = null, iconTint = Color.white, itemId = "hp_potion", maxStack = 10}
        };
        Debug.LogWarning("[ItemCatalogManager] itemCatalogEntries empty");
    }

    private void BuildCatalogDictionary()
    {
        catalogById.Clear();

        if (itemCatalogEntries == null)
        {
            Debug.Log("[ItemCatalogManager] itemCatalogEntries empty");
            return;
        }

        for (int i = 0; i < itemCatalogEntries.Length; i++)
        {
            ItemCatalogEntry entry = itemCatalogEntries[i];

            if (string.IsNullOrWhiteSpace(entry.itemId))
            {
                Debug.LogWarning($"[ItemCatalogManager] itemCatalogEntries[{i}] empty");
                continue;
            }

            string normalizedId = entry.itemId.Trim();
            if (catalogById.ContainsKey(normalizedId))
            {
                Debug.LogWarning($"[ItemCatalogManager] {normalizedId} duplicatedId");
                continue;
            }

            /*
            ItemCatalogEntry stored = entry;
            stored.id = normalizedId;
            catalogById.Add(normalizedId, stored);
            */

            catalogById.Add(normalizedId, entry);
        }
    }
}
