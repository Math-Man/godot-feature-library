using Godot;
using GodotFeatureLibrary.Interaction;
using GodotFeatureLibrary.GameInput;

namespace GodotFeatureLibrary.Note;

[GlobalClass]
public partial class Note : Area3D
{
    [Export] public NoteData Data { get; set; }

    [ExportGroup("Overrides")]
    [Export] public PackedScene LayoutOverride { get; set; }
    [Export] public AudioStream OpenSoundOverride { get; set; }
    [Export] public AudioStream CloseSoundOverride { get; set; }
    [Export] public AudioStream[] PageTurnSoundsOverride { get; set; }

    [ExportGroup("Typewriter Override")]
    [Export] public bool OverrideTypewriter { get; set; } = false;
    [Export] public bool UseTypewriter { get; set; } = true;

    [ExportGroup("Interaction")]
    [Export] public InspectableMode Mode { get; set; } = InspectableMode.World;
    [Export] public float MaxDistance { get; set; } = 3f;

    private const uint Layer3 = 4; // Layer 3

    public override void _Ready()
    {
        CollisionLayer = Layer3;
        CollisionMask = Layer3;
    }

    public override void _Input(InputEvent @event)
    {
        if (NoteService.Instance?.IsOpen == true) return;

        if (Mode == InspectableMode.Puzzle)
        {
            HandlePuzzleInput(@event);
        }
        else
        {
            HandleWorldInput(@event);
        }
    }

    private void HandlePuzzleInput(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Visible) return;
        if (!@event.IsActionPressed(InputMapping.INTERACTION_PUZZLE)) return;

        if (IsMouseOver())
        {
            NoteService.Instance?.OpenNote(Data, LayoutOverride, OpenSoundOverride, CloseSoundOverride, PageTurnSoundsOverride, OverrideTypewriter ? UseTypewriter : null);
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleWorldInput(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (!@event.IsActionPressed(InputMapping.INTERACTION_PRIMARY)) return;

        if (IsInCrosshair())
        {
            NoteService.Instance?.OpenNote(Data, LayoutOverride, OpenSoundOverride, CloseSoundOverride, PageTurnSoundsOverride, OverrideTypewriter ? UseTypewriter : null);
            GetViewport().SetInputAsHandled();
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
