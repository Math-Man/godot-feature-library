using Godot;
using GodotFeatureLibrary.DialogueEngine;
using GodotFeatureLibrary.GameInput;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Interaction;

public enum InspectableMode
{
    World,  // FPS-style: raycast from screen center + interact key
    Puzzle  // Mouse click when cursor is visible
}

[GlobalClass]
public partial class Inspectable : Area3D
{
    [Export] public InspectableMode Mode { get; set; } = InspectableMode.World;
    [Export] public float MaxDistance { get; set; } = 3f;
    [Export] public DialogueData[] Dialogues { get; set; } = [];

    private const uint Layer3 = 4; // Layer 3 = bit 2 = 4
    private int _currentDialogueIndex;

    private bool _interactionAllowed = true;

    public override void _Ready()
    {
        CollisionLayer = Layer3;
        CollisionMask = Layer3;
        EventBus.Instance?.Subscribe<InteractionAllowedChangedEvent>(e => _interactionAllowed = e.Allowed);
    }

    public override void _Input(InputEvent @event)
    {
        if (Mode == InspectableMode.Puzzle)
        {
            HandlePuzzleInput(@event);
        }
        else
        {
            HandleWorldInput(@event);
        }
    }

     // TODO: This requires a virtual mouse or snapping to elements on screen
    private void HandlePuzzleInput(InputEvent @event)
    {
        if (!_interactionAllowed) return;
        if (!@event.IsActionPressed(InputMapping.INTERACTION_PUZZLE)) return;

        if (IsMouseOver())
        {
            PublishDialogue();
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleWorldInput(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (!@event.IsActionPressed(InputMapping.INTERACTION_PRIMARY)) return;

        if (IsInCrosshair())
        {
            PublishDialogue();
            GetViewport().SetInputAsHandled();
        }
    }

    private void PublishDialogue()
    {
        if (Dialogues == null || Dialogues.Length == 0) return;

        var index = Mathf.Min(_currentDialogueIndex, Dialogues.Length - 1);
        var dialogue = Dialogues[index];

        if (dialogue != null)
        {
            EventBus.Instance.Publish(dialogue.ToEvent());
        }

        if (_currentDialogueIndex < Dialogues.Length - 1)
        {
            _currentDialogueIndex++;
        }
    }

    private bool IsMouseOver()
    {
        var mousePos = GetViewport().GetMousePosition();
        return IsPointOver(mousePos);
    }

    private bool IsInCrosshair()
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var center = viewportSize / 2;
        return IsPointOver(center);
    }

    private bool IsPointOver(Vector2 screenPoint)
    {
        var camera = GetViewport().GetCamera3D();
        if (camera == null) return false;

        var from = camera.ProjectRayOrigin(screenPoint);
        var to = from + camera.ProjectRayNormal(screenPoint) * MaxDistance;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = true;
        query.CollisionMask = CollisionLayer;

        var result = spaceState.IntersectRay(query);
        return result.Count > 0 && result["collider"].AsGodotObject() == this;
    }
}
