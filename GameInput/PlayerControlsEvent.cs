namespace GodotFeatureLibrary.GameInput;

public class PlayerControlsEvent
{
    public bool Enabled { get; }

    public PlayerControlsEvent(bool enabled)
    {
        Enabled = enabled;
    }
}