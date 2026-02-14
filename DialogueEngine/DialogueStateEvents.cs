namespace GodotFeatureLibrary.DialogueEngine;

public class DialogueStartedEvent
{
    public DialogueMode Mode { get; }
    public DialogueStartedEvent(DialogueMode mode) => Mode = mode;
}

public class DialogueDismissedEvent
{
    public DialogueMode Mode { get; }
    public DialogueDismissedEvent(DialogueMode mode) => Mode = mode;
}