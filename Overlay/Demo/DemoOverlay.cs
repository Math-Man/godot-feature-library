using Godot;

namespace GodotFeatureLibrary.Overlay.Demo;

public partial class DemoOverlay : Node
{
    [Export] public PackedScene OverlayScene { get; set; }
    [Export] public NodePath AnchorPath { get; set; }

    private Node3D _anchor;

    public override void _Ready()
    {
        if (AnchorPath != null && !AnchorPath.IsEmpty)
            _anchor = GetNode<Node3D>(AnchorPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } key) return;
        if (OverlayScene == null) return;

        var service = OverlayService.Instance;
        if (service == null) return;

        switch (key.Keycode)
        {
            case Key.Key1:
                service.Show(OverlayScene, new Vector2(400, 200), 2f, fadeIn: 0.3f, fadeOut: 0.5f);
                break;
            case Key.Key2 when _anchor != null:
                service.ShowAnchored(OverlayScene, _anchor, new Vector2(0, -50), 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                break;
            case Key.Key3 when _anchor != null:
                service.ShowBounds(OverlayScene, _anchor, new Vector2(8, 8), duration: 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                break;
            case Key.Key4 when _anchor != null:
                var mousePos = GetViewport().GetMousePosition();
                service.ShowLine(mousePos, _anchor.GlobalPosition, Colors.Green, 2f, 3f, fadeIn: 0.3f, fadeOut: 0.5f);
                break;
            case Key.Key5 when _anchor != null:
                var viewportSize = GetViewport().GetVisibleRect().Size;
                var center = viewportSize / 2f;
                var rectSize = new Vector2(120, 60);
                var screenRect = new Rect2(center - rectSize / 2f, rectSize);
                service.ShowBoundsConnector(screenRect, _anchor, Colors.Cyan, 2f, 4f, fadeIn: 0.3f, fadeOut: 0.5f);
                break;
        }
    }
}
