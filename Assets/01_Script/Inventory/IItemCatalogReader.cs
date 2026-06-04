public interface IItemCatalogReader
{
    public bool TryGetItemData(string itemId, out ItemData itemData);
    public bool IsRegistered(string itemId);
    public int GetMaxStack(string itemId);
    public string ResolveDisplayName(string itemId);    
}
