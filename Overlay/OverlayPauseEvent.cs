namespace GodotFeatureLibrary.Overlay;

public class OverlayPauseEvent
{
    public bool Paused { get; }

    public OverlayPauseEvent(bool paused)
    {
        Paused = paused;
    }
}
