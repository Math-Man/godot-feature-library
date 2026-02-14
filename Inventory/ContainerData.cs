using System;
using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.Inventory;

public class ContainerData
{
    public string Id { get; }
    public int SlotCount { get; }
    public ContainerMode Mode { get; set; }

    private readonly ItemStack[] _slots;

    public ContainerData(string id, int slotCount, ContainerMode mode = ContainerMode.Both)
    {
        Id = id;
        SlotCount = slotCount;
        Mode = mode;
        _slots = new ItemStack[slotCount];
    }

    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= SlotCount) return null;
        return _slots[index];
    }

    public IEnumerable<(int Index, ItemStack Stack)> GetOccupiedSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null)
                yield return (i, _slots[i]);
        }
    }

    /// <summary>
    /// Add items. Fills existing stacks first, then empty slots.
    /// Items with metadata always get their own slot (never merged).
    /// </summary>
    internal (int Added, int Remaining) TryAdd(
        string declarationId,
        int quantity,
        int maxStackSize,
        int maxPerContainer,
        Dictionary<string, Variant> metadata = null)
    {
        if (quantity <= 0) return (0, 0);

        int remaining = quantity;

        // Enforce max-per-container
        if (maxPerContainer > 0)
        {
            int currentTotal = CountItem(declarationId);
            int canAdd = maxPerContainer - currentTotal;
            if (canAdd <= 0) return (0, remaining);
            remaining = Math.Min(remaining, canAdd);
        }

        int toAdd = remaining;
        bool hasMetadata = metadata is { Count: > 0 };

        // Phase 1: Fill existing stacks (only for items without metadata)
        if (!hasMetadata)
        {
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.DeclarationId != declarationId) continue;
                if (slot.Metadata.Count > 0) continue;

                int space = maxStackSize - slot.Quantity;
                if (space <= 0) continue;

                int transfer = Math.Min(space, remaining);
                slot.Quantity += transfer;
                remaining -= transfer;
            }
        }

        // Phase 2: Create new stacks in empty slots
        for (int i = 0; i < _slots.Length && remaining > 0; i++)
        {
            if (_slots[i] != null) continue;

            int stackSize = Math.Min(maxStackSize, remaining);
            _slots[i] = new ItemStack(
                declarationId,
                stackSize,
                hasMetadata ? new Dictionary<string, Variant>(metadata) : null);
            remaining -= stackSize;
        }

        int added = toAdd - remaining;
        int totalRemaining = quantity - added;
        return (added, totalRemaining);
    }

    /// <summary>
    /// Remove items by declaration id. Takes from last slots first (LIFO).
    /// </summary>
    internal (int Removed, int Remaining) TryRemove(string declarationId, int quantity)
    {
        if (quantity <= 0) return (0, 0);

        int remaining = quantity;

        for (int i = _slots.Length - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = _slots[i];
            if (slot == null || slot.DeclarationId != declarationId) continue;

            int transfer = Math.Min(slot.Quantity, remaining);
            slot.Quantity -= transfer;
            remaining -= transfer;

            if (slot.Quantity <= 0)
                _slots[i] = null;
        }

        return (quantity - remaining, remaining);
    }

    internal ItemStack RemoveSlot(int index)
    {
        if (index < 0 || index >= SlotCount) return null;
        var stack = _slots[index];
        _slots[index] = null;
        return stack;
    }

    internal void SetSlot(int index, ItemStack stack)
    {
        if (index >= 0 && index < SlotCount)
            _slots[index] = stack;
    }

    internal void Clear()
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i] = null;
    }

    public int CountItem(string declarationId)
    {
        int count = 0;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i]?.DeclarationId == declarationId)
                count += _slots[i].Quantity;
        }
        return count;
    }

    public bool HasItem(string declarationId, int quantity = 1)
        => CountItem(declarationId) >= quantity;

    public int EmptySlotCount()
    {
        int count = 0;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null) count++;
        }
        return count;
    }
}
