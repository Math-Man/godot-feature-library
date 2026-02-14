namespace GodotFeatureLibrary.Inventory;

public class ItemAddedEvent
{
    public string ContainerId { get; }
    public string DeclarationId { get; }
    public int Quantity { get; }

    public ItemAddedEvent(string containerId, string declarationId, int quantity)
    {
        ContainerId = containerId;
        DeclarationId = declarationId;
        Quantity = quantity;
    }
}

public class ItemRemovedEvent
{
    public string ContainerId { get; }
    public string DeclarationId { get; }
    public int Quantity { get; }

    public ItemRemovedEvent(string containerId, string declarationId, int quantity)
    {
        ContainerId = containerId;
        DeclarationId = declarationId;
        Quantity = quantity;
    }
}

public class ContainerChangedEvent
{
    public string ContainerId { get; }
    public bool Registered { get; }

    public ContainerChangedEvent(string containerId, bool registered)
    {
        ContainerId = containerId;
        Registered = registered;
    }
}

public class InventoryOpenedEvent;
public class InventoryClosedEvent;
