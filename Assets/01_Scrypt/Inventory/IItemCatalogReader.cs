public interface IItemCatalogReader
{
    public bool TryGetEntry(string itemId, out ItemCatalogEntry entry);
    public bool IsRegistered(string itemId);
    public int GetMaxStack(string itemId);
    public string ResolveDisplayName(string itemId);    
}
