using System.Collections.Generic;
using Godot;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Overlay;

/// <summary>
/// Displays temporary visual overlays on screen, either at a fixed position
/// or anchored to a world object. Register as a script autoload.
/// </summary>
public partial class OverlayService : Node
{
    public static OverlayService Instance { get; private set; }

    private CanvasLayer _canvasLayer;
    private Control _container;
    private readonly List<ActiveOverlay> _overlays = new();
    private readonly List<ActiveLine> _lines = new();
    private readonly List<ActiveLineGroup> _lineGroups = new();
    private int _nextId;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _canvasLayer = new CanvasLayer { Layer = 90 };
        AddChild(_canvasLayer);

        _container = new Control();
        _container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _container.MouseFilter = Control.MouseFilterEnum.Ignore;
        _canvasLayer.AddChild(_container);

        EventBus.Instance?.Subscribe<OverlayPauseEvent>(e => { _canvasLayer.Visible = !e.Paused; });
    }

    public override void _Process(double delta)
    {
        var camera = GetViewport().GetCamera3D();
        float dt = (float)delta;
        ProcessOverlays(camera, dt);
        ProcessLines(camera, dt);
        ProcessLineGroups(camera, dt);
    }

    /// <summary>
    /// Cancel and remove a specific overlay or line by ID.
    /// </summary>
    public void Cancel(string id)
    {
        for (int i = _overlays.Count - 1; i >= 0; i--)
        {
            if (_overlays[i].Id == id)
            {
                RemoveOverlayAt(i);
                return;
            }
        }

        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            if (_lines[i].Id == id || _lines[i].GroupId == id)
                RemoveLineAt(i);
        }

        for (int i = _lineGroups.Count - 1; i >= 0; i--)
        {
            if (_lineGroups[i].Id == id)
            {
                RemoveLineGroupAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Cancel and remove all active overlays and lines.
    /// </summary>
    public void CancelAll()
    {
        for (int i = _overlays.Count - 1; i >= 0; i--)
            RemoveOverlayAt(i);
        for (int i = _lines.Count - 1; i >= 0; i--)
            RemoveLineAt(i);
        for (int i = _lineGroups.Count - 1; i >= 0; i--)
            RemoveLineGroupAt(i);
    }
}
