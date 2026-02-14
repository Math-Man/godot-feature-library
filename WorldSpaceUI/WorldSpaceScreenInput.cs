using Godot;

namespace GodotFeatureLibrary.WorldSpaceUI;

/// <summary>
/// Handles input injection for world-space UI screens.
/// Raycasts from a camera to the screen mesh and forwards input to the SubViewport.
/// Must be a child of a WorldSpaceScreen root node alongside SubViewport and ScreenMesh.
/// </summary>
public partial class WorldSpaceScreenInput : Node
{
    public enum PointerMode
    {
        /// <summary>Mouse cursor moves freely (requires mouse to be visible/uncaptured)</summary>
        FreeCursor,
        /// <summary>Uses screen center as pointer position (for FPS-style interaction)</summary>
        Crosshair
    }

    [Export] public PointerMode Mode { get; set; } = PointerMode.FreeCursor;
    [Export] public float RayLength { get; set; } = 10f;

    public bool Active { get; private set; } = false;
    public bool IsPointerOverScreen => _isOverScreen;

    private SubViewport _viewport;
    private MeshInstance3D _screenMesh;
    private Area3D _screenArea;

    private Vector2 _lastViewportPos = Vector2.Zero;
    private bool _isOverScreen = false;
    private Vector2 _quadSize = Vector2.One;

    public override void _Ready()
    {
        var root = GetParent();
        _viewport = root.GetNode<SubViewport>("SubViewport");
        _screenMesh = root.GetNode<MeshInstance3D>("ScreenMesh");
        _screenArea = _screenMesh.GetNode<Area3D>("ScreenArea");

        if (_screenMesh?.Mesh is QuadMesh quadMesh)
            _quadSize = quadMesh.Size;
    }

    public override void _Process(double delta)
    {
        var camera = GetViewport().GetCamera3D();

        bool shouldBeActive = camera != null;
        if (shouldBeActive != Active)
        {
            if (shouldBeActive) Activate();
            else Deactivate();
        }

        if (!Active) return;
        if (camera == null || _screenMesh == null || _viewport == null) return;

        var pointerScreenPos = GetPointerScreenPosition();
        var viewportPos = RaycastToViewportPosition(camera, pointerScreenPos, out bool hit);

        // Toggle system cursor when entering/leaving screen
        if (hit != _isOverScreen)
        {
            Input.MouseMode = hit ? Input.MouseModeEnum.Hidden : Input.MouseModeEnum.Visible;
        }

        _isOverScreen = hit;

        if (hit && viewportPos != _lastViewportPos)
        {
            InjectMouseMotion(viewportPos);
            _lastViewportPos = viewportPos;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Active || !_isOverScreen) return;

        if (@event is InputEventMouseButton mouseButton)
        {
            InjectMouseButton(mouseButton);
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Activates input handling for this screen.
    /// </summary>
    public void Activate()
    {
        Active = true;
        _isOverScreen = false;
    }

    /// <summary>
    /// Deactivates input handling for this screen.
    /// </summary>
    public void Deactivate()
    {
        Active = false;
        _isOverScreen = false;
    }

    private Vector2 GetPointerScreenPosition()
    {
        return Mode switch
        {
            PointerMode.FreeCursor => GetViewport().GetMousePosition(),
            PointerMode.Crosshair => GetViewport().GetVisibleRect().Size / 2f,
            _ => GetViewport().GetVisibleRect().Size / 2f
        };
    }

    private Vector2 RaycastToViewportPosition(Camera3D camera, Vector2 screenPos, out bool hit)
    {
        hit = false;

        var spaceState = camera.GetWorld3D().DirectSpaceState;
        var from = camera.ProjectRayOrigin(screenPos);
        var to = from + camera.ProjectRayNormal(screenPos) * RayLength;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        if (_screenArea != null)
        {
            query.CollisionMask = _screenArea.CollisionLayer;
        }

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return Vector2.Zero;

        // Verify we hit our specific screen area
        if (_screenArea != null)
        {
            var hitCollider = result["collider"].As<CollisionObject3D>();
            if (hitCollider != _screenArea) return Vector2.Zero;
        }

        hit = true;
        var worldHitPos = result["position"].AsVector3();
        return WorldPosToViewportPos(worldHitPos);
    }

    private Vector2 WorldPosToViewportPos(Vector3 worldPos)
    {
        var localPos = _screenMesh.ToLocal(worldPos);

        // QuadMesh: local X/Y range from -size/2 to +size/2
        // Convert to UV (0-1), Y flipped for UI coordinates
        var uv = new Vector2(
            (localPos.X / _quadSize.X) + 0.5f,
            0.5f - (localPos.Y / _quadSize.Y)
        );

        uv = uv.Clamp(Vector2.Zero, Vector2.One);
        return uv * _viewport.Size;
    }

    private void InjectMouseMotion(Vector2 position)
    {
        var ev = new InputEventMouseMotion
        {
            Position = position,
            GlobalPosition = position
        };
        _viewport.PushInput(ev, true);
    }

    private void InjectMouseButton(InputEventMouseButton original)
    {
        var ev = new InputEventMouseButton
        {
            Position = _lastViewportPos,
            GlobalPosition = _lastViewportPos,
            ButtonIndex = original.ButtonIndex,
            Pressed = original.Pressed,
            DoubleClick = original.DoubleClick
        };
        _viewport.PushInput(ev, true);
    }
}
