using Godot;
using GodotFeatureLibrary.Interaction;

namespace GodotFeatureLibrary.Inventory;

[GlobalClass]
public partial class ItemPickup : Area3D, IInteractable
{
    [Export] public ItemDeclaration Declaration { get; set; }
    [Export] public int Quantity { get; set; } = 1;
    [Export] public string TargetContainer { get; set; } = "player";
    [Export] public Node PickupRoot { get; set; }
    [Export] public bool RequireCapturedCursor { get; set; } = true;
    bool IInteractable.RequireCapturedCursor => RequireCapturedCursor;

    private const uint InteractionLayer = 2;

    public override void _Ready()
    {
        CollisionLayer = InteractionLayer;
        CollisionMask = 0;
    }

    public void OnInteract()
    {
        if (Declaration == null)
        {
            GD.PushError("ItemPickup: No declaration assigned");
            return;
        }

        var service = InventoryService.Instance;
        if (service == null)
        {
            GD.PushError("ItemPickup: InventoryService not found");
            return;
        }

        var result = service.AddItem(TargetContainer, Declaration.Id, Quantity);

        if (result.Success)
        {
            GD.Print($"[ItemPickup] Picked up {result.QuantityAffected}x {Declaration.DisplayName}");
            (PickupRoot ?? this).QueueFree();
        }
        else
        {
            GD.Print($"[ItemPickup] Cannot pick up: {result.Error}");
        }
    }
}
