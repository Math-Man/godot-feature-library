using Godot;

namespace GodotFeatureLibrary.Interaction.Demo;

/// <summary>
/// Basic mouse look for demo scenes. Attach to a Camera3D.
/// Captures mouse on ready, press Escape to release.
/// </summary>
public partial class DemoMouseLook : Camera3D
{
    [Export] public float Sensitivity { get; set; } = 0.002f;
    [Export] public float MoveSpeed { get; set; } = 3f;

    private float _pitch;
    private float _yaw;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _yaw -= motion.Relative.X * Sensitivity;
            _pitch -= motion.Relative.Y * Sensitivity;
            _pitch = Mathf.Clamp(_pitch, -Mathf.Pi / 2f, Mathf.Pi / 2f);

            Rotation = new Vector3(_pitch, _yaw, 0);
        }

        if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;

        var dir = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W)) dir -= Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.S)) dir += Transform.Basis.Z;
        if (Input.IsKeyPressed(Key.A)) dir -= Transform.Basis.X;
        if (Input.IsKeyPressed(Key.D)) dir += Transform.Basis.X;

        if (dir != Vector3.Zero)
            Position += dir.Normalized() * MoveSpeed * (float)delta;
    }
}
