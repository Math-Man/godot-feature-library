using Godot;
using Godot.Collections;

namespace GodotFeatureLibrary.Inventory;

[GlobalClass]
public partial class ItemDeclaration : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public Dictionary<string, string> Descriptions { get; set; } = new();
    [Export] public Texture2D Icon { get; set; }

    [ExportGroup("Stacking")]
    [Export] public int MaxStackSize { get; set; } = 1;
    [Export] public int MaxPerContainer { get; set; } = -1;

    [ExportGroup("World")]
    [Export] public PackedScene WorldScene { get; set; }
    [Export] public bool Droppable { get; set; } = true;

    [ExportGroup("Tags")]
    [Export] public string[] Tags { get; set; } = [];
}
