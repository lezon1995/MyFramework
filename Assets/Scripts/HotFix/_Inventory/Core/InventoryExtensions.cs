namespace MoreMountains.InventoryEngine
{
    public static class InventoryExtensions
    {
        public static bool IsNull(this InventoryItem item)
        {
            if (item == null)
                return true;

            if (string.IsNullOrEmpty(item.ItemID))
                return true;

            return false;
        }
    }
}