using Godot;

namespace GodotFeatureLibrary;

public partial class MainMenu : Control
{
    private static readonly (string Name, string Path)[] Demos =
    {
        ("Dialogue Engine", "res://DialogueEngine/dialogue_demo.tscn"),
        ("Debug Console", "res://DebugConsole/demo/debug_console_demo.tscn"),
        ("Interaction", "res://Interaction/Demo/interaction_demo.tscn"),
        ("Inventory", "res://Inventory/Demo/inventory_demo.tscn"),
        ("WorldSpace UI", "res://WorldSpaceUI/Demo/worldspaceui_demo.tscn"),
        ("Post Process", "res://PostProcess/Demo/postprocess_demo.tscn"),
        ("Overlay", "res://Overlay/Demo/overlay_demo.tscn"),
        ("Note", "res://Note/Demo/note_demo.tscn"),
        ("Puzzle", "res://Puzzle/Demo/puzzle_demo.tscn"),
    };

    public override void _Ready()
    {
        var container = GetNode<VBoxContainer>("Center/VBox");

        foreach (var (name, path) in Demos)
        {
            var button = new Button
            {
                Text = name,
                CustomMinimumSize = new Vector2(260, 36)
            };
            button.Pressed += () => GetTree().ChangeSceneToFile(path);
            container.AddChild(button);
        }
    }
}
