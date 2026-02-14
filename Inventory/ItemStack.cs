using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.Inventory;

public class ItemStack
{
    public string DeclarationId { get; }
    public int Quantity { get; internal set; }
    public Dictionary<string, Variant> Metadata { get; }

    public ItemStack(string declarationId, int quantity = 1, Dictionary<string, Variant> metadata = null)
    {
        DeclarationId = declarationId;
        Quantity = quantity;
        Metadata = metadata ?? new Dictionary<string, Variant>();
    }

    public ItemStack Clone()
    {
        var clonedMetadata = new Dictionary<string, Variant>(Metadata);
        return new ItemStack(DeclarationId, Quantity, clonedMetadata);
    }
}
