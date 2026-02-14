using Godot;

namespace GodotFeatureLibrary.Inventory.Demo;

/// <summary>
/// Sets up the inventory demo. Creates a player container on ready.
/// Add to the demo scene root.
/// </summary>
public partial class DemoInventorySetup : Node
{
    [Export] public int PlayerSlots { get; set; } = 10;

    public override void _Ready()
    {
        // Wait a frame for InventoryService to initialize
        CallDeferred(MethodName.Setup);
    }

    private void Setup()
    {
        var service = InventoryService.Instance;
        if (service == null)
        {
            GD.PushError("[InventoryDemo] InventoryService not found. Add it to the scene or as an autoload.");
            return;
        }

        service.CreateContainer("player", PlayerSlots);
        GD.Print($"[InventoryDemo] Created player container with {PlayerSlots} slots");
    }
}
