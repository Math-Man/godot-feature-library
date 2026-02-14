using Godot;
using GodotFeatureLibrary.DialogueEngine;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.WorldSpaceUI;

public partial class TerminalButton : Button
{
	public override void _Pressed()
	{
		EventBus.Instance.Publish(new DialogueEvent(
			"Nothing happened.",
			DialogueMode.Narration,
			duration: 1f,
			lingerDuration: 1.0f
		));
	}
}
