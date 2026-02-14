using Godot;

namespace GodotFeatureLibrary.Interaction.Demo;

public partial class DemoInteractableMovement : Node
{

	public override void _Process(double delta)
	{
		Node3D parent = GetParent<Node3D>();
		parent.Rotate(parent.GetGlobalBasis() * Vector3.Up , (float)delta * 1.5f);
	}
}