using Godot;
using GodotFeatureLibrary.DebugConsole;

namespace GodotFeatureLibrary.Overlay;

/// <summary>
/// Test trigger for overlay system.
/// Registers debug console commands and keyboard shortcuts for testing overlays.
/// </summary>
public partial class OverlayTestTrigger : Node
{
    [Export] private PackedScene _testScene;
    [Export] private Node3D _testAnchor;

    public override void _Ready()
    {
        DebugConsoleService.Instance?.RegisterCommand("overlay", "Test overlays: screen, anchored, bounds, line, connector", args =>
        {
            if (args.Length == 0)
                return "Usage: overlay <screen|anchored|bounds|line|connector>";

            var service = OverlayService.Instance;
            if (service == null) return "OverlayService not available";

            switch (args[0].ToLowerInvariant())
            {
                case "screen":
                    service.Show(_testScene, new Vector2(420, 250), 1.5f, fadeIn: 0.3f, fadeOut: 0.5f);
                    return "Screen overlay spawned";
                case "anchored":
                    if (_testAnchor == null) return "No test anchor set";
                    service.ShowAnchored(_testScene, _testAnchor, new Vector2(0, -40), 2f, fadeIn: 0.3f, fadeOut: 0.5f);
                    return "Anchored overlay spawned";
                case "bounds":
                    if (_testAnchor == null) return "No test anchor set";
                    service.ShowBounds(_testScene, _testAnchor, new Vector2(8, 8), maxDistance: 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                    return "Bounds overlay spawned";
                case "line":
                    if (_testAnchor == null) return "No test anchor set";
                    var mousePos = GetViewport().GetMousePosition();
                    service.ShowLine(mousePos, _testAnchor.GlobalPosition, Colors.Green, 2f, 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                    return "Line spawned";
                case "connector":
                    if (_testAnchor == null) return "No test anchor set";
                    var vp = GetViewport().GetVisibleRect().Size;
                    var screenRect = new Rect2(vp.X * 0.05f, vp.Y * 0.1f, 120, 80);
                    service.ShowBoundsConnector(screenRect, _testAnchor, Colors.Cyan, 2f, 4f, fadeIn: 0.3f, fadeOut: 0.5f);
                    return "Bounds connector spawned";
                default:
                    return $"Unknown overlay type: {args[0]}. Options: screen, anchored, bounds, line, connector";
            }
        });
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } key) return;

        var service = OverlayService.Instance;
        if (service == null) return;

        switch (key.Keycode)
        {
            case Key.Key8:
                service.Show(_testScene, new Vector2(420, 250), 1.5f, fadeIn: 0.3f, fadeOut: 0.5f);
                GetViewport().SetInputAsHandled();
                break;

            case Key.Key7:
                if (_testAnchor != null)
                    service.ShowAnchored(_testScene, _testAnchor, new Vector2(0, -40), 2f, fadeIn: 0.3f, fadeOut: 0.5f);
                GetViewport().SetInputAsHandled();
                break;

            case Key.Key6:
                if (_testAnchor != null)
                    service.ShowBounds(_testScene, _testAnchor, new Vector2(8, 8), maxDistance: 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                GetViewport().SetInputAsHandled();
                break;

            case Key.Key5:
                var viewportSize = GetViewport().GetVisibleRect().Size;
                if (_testAnchor != null)
                    service.ShowLine(viewportSize / 2f, _testAnchor.GlobalPosition, Colors.Green, 2f, 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                GetViewport().SetInputAsHandled();
                break;

            case Key.Key4:
                if (_testAnchor != null)
                {
                    var vp = GetViewport().GetVisibleRect().Size;
                    var screenRect = new Rect2(vp.X * 0.05f, vp.Y * 0.1f, 120, 80);
                    service.ShowBoundsConnector(screenRect, _testAnchor, Colors.Cyan, 2f, 4f, fadeIn: 0.3f, fadeOut: 0.5f);
                }
                GetViewport().SetInputAsHandled();
                break;
        }
    }
}
