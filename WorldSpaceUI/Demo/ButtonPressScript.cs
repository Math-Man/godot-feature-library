using Godot;
using System;

public partial class ButtonPressScript : Button
{
	public override void _Ready()
	{
		this.Pressed += () =>
		{
			GD.Print("Button was pressed!");
		};
	}

}
