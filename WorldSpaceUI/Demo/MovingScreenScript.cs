using Godot;

namespace GodotFeatureLibrary.WorldSpaceUI.Demo;

public partial class MovingScreenScript: Node
{
    public override void _Process(double delta)
    {
        Node3D parent = GetParent<Node3D>();
        // rotate in global y axis between -15 and 15 degrees
        float angle = (float)(Mathf.Sin(Time.GetTicksMsec() / 1000.0) * Mathf.DegToRad(15));
        parent.Rotation = new Vector3(parent.Rotation.X, angle, parent.Rotation.Z);
    }
}