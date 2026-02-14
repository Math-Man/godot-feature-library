namespace GodotFeatureLibrary.Interaction;

public interface IInteractable
{
    bool RequireFreeCursor => false;
    bool RequireCapturedCursor => false;
    void OnInteract();
}
