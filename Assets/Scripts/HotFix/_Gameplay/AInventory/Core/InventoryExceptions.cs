using System;

namespace MoreMountains
{
    public class InventoryFullException : Exception
    {
        public ItemKind BagKind { get; }

        public InventoryFullException(ItemKind kind)
            : base($"{kind} bag is full")
        {
            BagKind = kind;
        }
    }

    public class InventoryItemNotFoundException : Exception
    {
        public InventoryItemNotFoundException(IInventoryItem item)
            : base($"Item not found in bag: {item?.DisplayName ?? "<null>"}")
        {
        }
    }

    public class InventoryExpansionLimitException : Exception
    {
        public InventoryExpansionLimitException(string bag, int requested, int max)
            : base($"{bag} bag expand rejected, requested {requested}, max {max}")
        {
        }
    }

    public class InventoryShrinkInvalidException : Exception
    {
        public InventoryShrinkInvalidException(string bag, int delta, int available)
            : base($"{bag} bag shrink rejected, need {delta} empty trailing slots, only {available} available")
        {
        }
    }
}