using Godot;

namespace GodotFeatureLibrary.Interaction.Demo;

/// <summary>
/// Simple IInteractable demo. Changes color on interact.
/// Attach to an Area3D on collision layer 2.
/// </summary>
public partial class DemoInteractable : Area3D, IInteractable
{
    [Export] public bool RequireFreeCursor { get; set; }
    bool IInteractable.RequireFreeCursor => RequireFreeCursor;

    private bool _toggled;

    public void OnInteract()
    {
        _toggled = !_toggled;

        // Find first MeshInstance3D child to recolor
        MeshInstance3D mesh = null;
        foreach (var child in GetChildren())
        {
            if (child is MeshInstance3D m) { mesh = m; break; }
        }
        if (mesh != null)
        {
            var mat = new StandardMaterial3D
            {
                AlbedoColor = _toggled ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.8f, 0.3f, 0.2f)
            };
            mesh.MaterialOverride = mat;
        }

        GD.Print($"[DemoInteractable] Interacted! Toggled: {_toggled}");
    }
}
