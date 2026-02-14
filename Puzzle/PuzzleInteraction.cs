using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using GodotFeatureLibrary.Interaction;
using GodotFeatureLibrary.GameInput;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Puzzle;

/// <summary>
/// Attach to a puzzle object (e.g., texture_test_built root).
/// Handles entering/exiting puzzle mode with camera switch and detail swap.
/// </summary>
public partial class PuzzleInteraction : Node, IInteractable
{
    [ExportGroup("Camera")]
    [Export] public Camera3D PuzzleCamera { get; set; }

    [ExportGroup("Detail Swap")]
    [Export] public string GeometryRootName { get; set; }
    [Export] public string LowDetailSuffix { get; set; } = "_low";
    [Export] public string HighDetailSuffix { get; set; } = "_high";

    [ExportGroup("Transition")]
    [Export] public float FadeDuration { get; set; } = 0.3f;
    [Export] public Color FadeColor { get; set; } = Colors.Black;

    private bool _inPuzzleMode;
    private bool _isTransitioning;
    private Camera3D _previousCamera;
    private ColorRect _fadeRect;
    private Node3D[] _lowDetailNodes;
    private Node3D[] _highDetailNodes;

    public override void _Ready()
    {
        FindDetailNodes();
        SetDetailMode(highDetail: false);
        CreateFadeOverlay();
    }

    private void FindDetailNodes()
    {
        if (string.IsNullOrEmpty(GeometryRootName))
        {
            _lowDetailNodes = [];
            _highDetailNodes = [];
            return;
        }

        var sceneRoot = GetTree().CurrentScene;
        var root = FindNodeByName(sceneRoot, GeometryRootName);
        if (root == null)
        {
            GD.PrintErr($"PuzzleInteraction: Could not find geometry root '{GeometryRootName}'");
            _lowDetailNodes = [];
            _highDetailNodes = [];
            return;
        }

        _lowDetailNodes = FindNodesBySuffix(root, LowDetailSuffix);
        _highDetailNodes = FindNodesBySuffix(root, HighDetailSuffix);

        GD.Print($"PuzzleInteraction: Found {_lowDetailNodes.Length} low detail, {_highDetailNodes.Length} high detail nodes");
    }

    private static Node FindNodeByName(Node root, string name)
    {
        if (root.Name == name) return root;

        foreach (var child in root.GetChildren())
        {
            var found = FindNodeByName(child, name);
            if (found != null) return found;
        }

        return null;
    }

    private static Node3D[] FindNodesBySuffix(Node root, string suffix)
    {
        if (string.IsNullOrEmpty(suffix)) return [];

        var results = new List<Node3D>();
        FindNodesBySuffixRecursive(root, suffix, results);
        return results.ToArray();
    }

    private static void FindNodesBySuffixRecursive(Node node, string suffix, List<Node3D> results)
    {
        if (node is Node3D node3D && node.Name.ToString().EndsWith(suffix))
        {
            results.Add(node3D);
        }

        foreach (var child in node.GetChildren())
        {
            FindNodesBySuffixRecursive(child, suffix, results);
        }
    }

    private void CreateFadeOverlay()
    {
        var viewport = GetViewport();
        var size = viewport.GetVisibleRect().Size;

        _fadeRect = new ColorRect
        {
            Color = FadeColor with { A = 0f },
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = size,
            Position = Vector2.Zero
        };

        var canvasLayer = new CanvasLayer { Layer = 100 };
        canvasLayer.AddChild(_fadeRect);
        GetTree().Root.CallDeferred(Node.MethodName.AddChild, canvasLayer);
    }

    public override void _Input(InputEvent @event)
    {
        if (!_inPuzzleMode || _isTransitioning) return;

        if (@event.IsActionPressed(InputMapping.INTERACTION_SECONDARY))
        {
            ExitPuzzleMode();
            GetViewport().SetInputAsHandled();
        }
    }

    public void OnInteract() => EnterPuzzleMode();

    public async void EnterPuzzleMode()
    {
        if (_inPuzzleMode || _isTransitioning) return;
        if (PuzzleCamera == null)
        {
            GD.PrintErr("PuzzleInteraction: No PuzzleCamera assigned");
            return;
        }

        _isTransitioning = true;
        _inPuzzleMode = true;

        _previousCamera = GetViewport().GetCamera3D();

        await FadeToBlack();

        PuzzleCamera.Current = true;
        SetDetailMode(highDetail: true);

        if (GetParent() is Area3D area) area.CollisionLayer = 0;

        EventBus.Instance?.Publish(new PuzzleEnteredEvent());

        await FadeFromBlack();

        _isTransitioning = false;
        GD.Print("Entered puzzle mode");
    }

    public async void ExitPuzzleMode()
    {
        if (!_inPuzzleMode || _isTransitioning) return;

        _isTransitioning = true;

        await FadeToBlack();

        _inPuzzleMode = false;

        if (_previousCamera != null)
        {
            _previousCamera.Current = true;
        }

        SetDetailMode(highDetail: false);

        if (GetParent() is Area3D exitArea) exitArea.CollisionLayer = 2;

        EventBus.Instance?.Publish(new PuzzleExitedEvent());

        await FadeFromBlack();

        _isTransitioning = false;
        GD.Print("Exited puzzle mode");
    }

    private async Task FadeToBlack()
    {
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1f, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private async Task FadeFromBlack()
    {
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 0f, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private void SetDetailMode(bool highDetail)
    {
        if (_lowDetailNodes != null)
        {
            foreach (var node in _lowDetailNodes)
                node?.SetVisible(!highDetail);
        }

        if (_highDetailNodes != null)
        {
            foreach (var node in _highDetailNodes)
                node?.SetVisible(highDetail);
        }
    }
}
