using Godot;

namespace GodotFeatureLibrary.DialogueEngine;

[GlobalClass]
public partial class DialogueData : Resource
{
    [Export] public string Content { get; set; }
    [Export] public DialogueMode Mode { get; set; } = DialogueMode.Dialogue;
    [Export] public Curve TypewriterCurve { get; set; }
    [Export] public float Duration { get; set; } = 2f;
    [Export] public float LingerDuration { get; set; } = 1f;
    [Export] public string Title { get; set; }
    [Export] public Color TitleColor { get; set; } = new Color(0, 0, 0, 0);
    [Export] public bool Override { get; set; }

    public DialogueEvent ToEvent()
    {
        return new DialogueEvent(
            Content,
            Mode,
            TypewriterCurve,
            Duration,
            LingerDuration,
            Title,
            TitleColor.A > 0 ? TitleColor : null,
            Override
        );
    }
}