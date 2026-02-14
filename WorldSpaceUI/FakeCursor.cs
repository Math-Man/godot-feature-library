using Godot;

namespace GodotFeatureLibrary.WorldSpaceUI;

/// <summary>
/// A cursor that follows injected mouse input within a SubViewport.
/// Add as a child of your world-space UI, set Mouse Filter to Ignore.
/// </summary>
public partial class FakeCursor : TextureRect
{
    public override void _Ready()
    {
        // Ensure it draws on top
        ZIndex = 100;
        // Don't block mouse events
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            // Center the cursor on the mouse position (account for scale)
            Position = motion.Position - Size * Scale / 2;
        }
    }
}
