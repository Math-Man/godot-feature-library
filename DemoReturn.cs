using Godot;

namespace GodotFeatureLibrary;

/// <summary>
/// Autoload that returns to the main menu when Backspace is pressed.
/// </summary>
public partial class DemoReturn : Node
{
    private const string MainMenuPath = "res://main_menu.tscn";

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Keycode: Key.Backspace }) return;

        var current = GetTree().CurrentScene?.SceneFilePath;
        if (current == MainMenuPath) return;

        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile(MainMenuPath);
    }
}
