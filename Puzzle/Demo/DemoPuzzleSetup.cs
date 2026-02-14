using Godot;

namespace GodotFeatureLibrary.Puzzle.Demo;

public partial class DemoPuzzleSetup : Node
{
    public override void _Ready()
    {
        var interaction = GetNode<PuzzleInteraction>("../PuzzleTrigger/PuzzleInteraction");
        interaction.PuzzleCamera = GetNode<Camera3D>("../PuzzleCamera");
    }
}
