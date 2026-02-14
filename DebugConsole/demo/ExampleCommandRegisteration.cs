using Godot;

namespace GodotFeatureLibrary.DebugConsole.demo;

public partial class ExampleCommandRegisteration : Node
{
	public override void _Ready()
	{
		DebugConsoleService.Instance.RegisterCommand("greet", "Greets the player with a message.", args =>
		{
			var name = args.Length > 0 ? args[0] : "Player";
			GD.Print($"Hello, {name}!");
			return $"Greeted {name}";
		});
	}

}