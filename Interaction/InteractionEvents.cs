namespace GodotFeatureLibrary.Interaction;

public class InteractionAllowedChangedEvent
{
    public bool Allowed { get; }
    public InteractionAllowedChangedEvent(bool allowed) => Allowed = allowed;
}
