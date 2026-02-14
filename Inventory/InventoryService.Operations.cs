using System.Collections.Generic;
using Godot;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Inventory;

public partial class InventoryService
{
    public InventoryResult AddItem(
        string containerId,
        string declarationId,
        int quantity = 1,
        Dictionary<string, Variant> metadata = null)
    {
        if (!_containers.TryGetValue(containerId, out var container))
            return InventoryResult.Fail($"Container '{containerId}' not found");

        if (container.Mode == ContainerMode.ExtractOnly)
            return InventoryResult.Fail($"Container '{containerId}' is extract-only");

        var declaration = GetDeclaration(declarationId);
        if (declaration == null)
            return InventoryResult.Fail($"Item declaration '{declarationId}' not found");

        var (added, remaining) = container.TryAdd(
            declarationId, quantity, declaration.MaxStackSize, declaration.MaxPerContainer, metadata);

        if (added > 0)
            EventBus.Instance?.Publish(new ItemAddedEvent(containerId, declarationId, added));

        if (remaining > 0)
            return added > 0
                ? InventoryResult.Partial(added, remaining)
                : InventoryResult.Fail("No space available");

        return InventoryResult.Ok(added);
    }

    public InventoryResult RemoveItem(string containerId, string declarationId, int quantity = 1)
    {
        if (!_containers.TryGetValue(containerId, out var container))
            return InventoryResult.Fail($"Container '{containerId}' not found");

        if (container.Mode == ContainerMode.InsertOnly)
            return InventoryResult.Fail($"Container '{containerId}' is insert-only");

        var (removed, remaining) = container.TryRemove(declarationId, quantity);

        if (removed > 0)
            EventBus.Instance?.Publish(new ItemRemovedEvent(containerId, declarationId, removed));

        if (remaining > 0)
            return removed > 0
                ? InventoryResult.Partial(removed, remaining)
                : InventoryResult.Fail($"Item '{declarationId}' not found in container");

        return InventoryResult.Ok(removed);
    }

    public ItemStack RemoveSlot(string containerId, int slotIndex)
    {
        if (!_containers.TryGetValue(containerId, out var container))
            return null;

        if (container.Mode == ContainerMode.InsertOnly)
            return null;

        var stack = container.RemoveSlot(slotIndex);
        if (stack != null)
            EventBus.Instance?.Publish(new ItemRemovedEvent(containerId, stack.DeclarationId, stack.Quantity));

        return stack;
    }

    public InventoryResult TransferItem(
        string sourceContainerId,
        string destContainerId,
        string declarationId,
        int quantity = 1)
    {
        var source = GetContainer(sourceContainerId);
        var dest = GetContainer(destContainerId);

        if (source == null) return InventoryResult.Fail($"Source container '{sourceContainerId}' not found");
        if (dest == null) return InventoryResult.Fail($"Destination container '{destContainerId}' not found");
        if (source.Mode == ContainerMode.InsertOnly) return InventoryResult.Fail("Source is insert-only");
        if (dest.Mode == ContainerMode.ExtractOnly) return InventoryResult.Fail("Destination is extract-only");

        if (!source.HasItem(declarationId, quantity))
            return InventoryResult.Fail($"Source doesn't have {quantity}x '{declarationId}'");

        var addResult = AddItem(destContainerId, declarationId, quantity);
        if (addResult.QuantityAffected > 0)
            RemoveItem(sourceContainerId, declarationId, addResult.QuantityAffected);

        return addResult;
    }

    public bool HasItem(string containerId, string declarationId, int quantity = 1)
    {
        var container = GetContainer(containerId);
        return container?.HasItem(declarationId, quantity) ?? false;
    }

    public int GetCount(string containerId, string declarationId)
    {
        var container = GetContainer(containerId);
        return container?.CountItem(declarationId) ?? 0;
    }

    public bool HasItemAnywhere(string declarationId, int quantity = 1)
    {
        int total = 0;
        foreach (var container in _containers.Values)
        {
            total += container.CountItem(declarationId);
            if (total >= quantity) return true;
        }
        return false;
    }
}
