using Godot;

namespace GodotFeatureLibrary.DialogueEngine;

public class DialogueEvent
{
    public string Title { get; }
    public Color? TitleColor { get; }
    public string Content { get; }
    public Curve TypewriterCurve { get; }
    public DialogueMode Mode { get; }
    public float Duration { get; }
    public float LingerDuration { get; }
    public bool Override { get; }

    public DialogueEvent(
        string content,
        DialogueMode mode = DialogueMode.Dialogue,
        Curve typewriterCurve = null,
        float duration = 2f,
        float lingerDuration = 1f,
        string title = null,
        Color? titleColor = null,
        bool @override = false)
    {
        Content = content;
        Mode = mode;
        TypewriterCurve = typewriterCurve;
        Duration = duration;
        LingerDuration = lingerDuration;
        Title = title;
        TitleColor = titleColor;
        Override = @override;
    }
}