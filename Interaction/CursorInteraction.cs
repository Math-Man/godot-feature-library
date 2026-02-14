using Godot;
using GodotFeatureLibrary.GameInput;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Interaction;

/// <summary>
/// Raycasts from the active camera at the mouse position when the cursor is visible.
/// Finds IInteractable on click — the free-cursor equivalent of PlayerInteractionBehavior.
/// Add as a script autoload.
/// </summary>
public partial class CursorInteraction : Node
{
    private const float RayLength = 50f;
    private const uint InteractionLayer = 2;
    private bool _interactionAllowed = true;

    public override void _Ready()
    {
        EventBus.Instance?.Subscribe<InteractionAllowedChangedEvent>(e => _interactionAllowed = e.Allowed);
    }

    public override void _Input(InputEvent @event)
    {
        if (!_interactionAllowed) return;
        if (!@event.IsActionPressed(InputMapping.INTERACTION_PRIMARY)) return;

        var camera = GetViewport().GetCamera3D();
        if (camera == null) return;

        var mousePos = GetViewport().GetMousePosition();
        var from = camera.ProjectRayOrigin(mousePos);
        var to = from + camera.ProjectRayNormal(mousePos) * RayLength;

        var spaceState = camera.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;
        query.CollisionMask = InteractionLayer;

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return;

        var collider = result["collider"].As<Node>();
        var interactable = FindInteractable(collider);
        if (interactable == null) return;
        if (interactable.RequireFreeCursor && Input.MouseMode != Input.MouseModeEnum.Visible) return;
        if (interactable.RequireCapturedCursor && Input.MouseMode != Input.MouseModeEnum.Captured) return;
        interactable.OnInteract();
    }

    private static IInteractable FindInteractable(Node node)
    {
        var current = node;
        while (current != null)
        {
            if (current is IInteractable interactable) return interactable;

            foreach (var child in current.GetChildren())
            {
                if (child is IInteractable childInteractable) return childInteractable;
            }

            current = current.GetParent();
        }
        return null;
    }
}
