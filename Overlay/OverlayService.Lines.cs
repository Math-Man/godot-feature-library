using Godot;

namespace GodotFeatureLibrary.Overlay;

public partial class OverlayService
{
    /// <summary>
    /// Show a line between two world positions, projected to screen each frame.
    /// Duration &lt;= 0 means infinite (must be cancelled manually).
    /// </summary>
    public string ShowLine(Vector3 fromWorld, Vector3 toWorld, Color color, float width = 2f, float duration = 0f,
        float fadeIn = 0f, float fadeOut = 0f)
    {
        var line = new Line2D();
        line.Width = width;
        line.DefaultColor = color;
        line.AddPoint(Vector2.Zero);
        line.AddPoint(Vector2.Zero);
        _canvasLayer.AddChild(line);

        var id = $"line_{_nextId++}";
        _lines.Add(new ActiveLine
        {
            Id = id,
            Instance = line,
            FromWorld = fromWorld,
            ToWorld = toWorld,
            Color = color,
            Duration = duration,
            FadeIn = fadeIn,
            FadeOut = fadeOut,
        });

        return id;
    }

    /// <summary>
    /// Show a line from a fixed screen position to a world position projected each frame.
    /// Duration &lt;= 0 means infinite (must be cancelled manually).
    /// </summary>
    public string ShowLine(Vector2 fromScreen, Vector3 toWorld, Color color, float width = 2f, float duration = 0f,
        float fadeIn = 0f, float fadeOut = 0f)
    {
        var line = new Line2D();
        line.Width = width;
        line.DefaultColor = color;
        line.AddPoint(fromScreen);
        line.AddPoint(Vector2.Zero);
        _canvasLayer.AddChild(line);

        var id = $"line_{_nextId++}";
        _lines.Add(new ActiveLine
        {
            Id = id,
            Instance = line,
            ToWorld = toWorld,
            FixedFromScreen = fromScreen,
            Color = color,
            Duration = duration,
            FadeIn = fadeIn,
            FadeOut = fadeOut,
        });

        return id;
    }

    /// <summary>
    /// Draw lines connecting screen points to world points.
    /// When autoPair is true, lines re-match to nearest projected world point each frame.
    /// When false, pairs are fixed by index.
    /// Returns a group ID that cancels all lines at once.
    /// </summary>
    public string ShowLines(Vector2[] fromScreen, Vector3[] toWorld, Color color, float width = 2f, float duration = 0f,
        float fadeIn = 0f, float fadeOut = 0f, bool autoPair = false)
    {
        var groupId = $"linegroup_{_nextId++}";
        int count = Mathf.Min(fromScreen.Length, toWorld.Length);

        if (autoPair)
        {
            var instances = new System.Collections.Generic.List<Line2D>(count);
            for (int i = 0; i < count; i++)
            {
                var line = new Line2D();
                line.Width = width;
                line.DefaultColor = color;
                line.AddPoint(fromScreen[i]);
                line.AddPoint(Vector2.Zero);
                _canvasLayer.AddChild(line);
                instances.Add(line);
            }

            _lineGroups.Add(new ActiveLineGroup
            {
                Id = groupId,
                Instances = instances,
                FromScreen = fromScreen,
                ToWorld = toWorld,
                Duration = duration,
                FadeIn = fadeIn,
                FadeOut = fadeOut,
            });
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                var line = new Line2D();
                line.Width = width;
                line.DefaultColor = color;
                line.AddPoint(fromScreen[i]);
                line.AddPoint(Vector2.Zero);
                _canvasLayer.AddChild(line);

                _lines.Add(new ActiveLine
                {
                    Id = $"line_{_nextId++}",
                    GroupId = groupId,
                    Instance = line,
                    ToWorld = toWorld[i],
                    FixedFromScreen = fromScreen[i],
                    Color = color,
                    Duration = duration,
                    FadeIn = fadeIn,
                    FadeOut = fadeOut,
                });
            }
        }

        return groupId;
    }

    /// <summary>
    /// Draw auto-paired lines from the corners of a screen rect to a world object's AABB.
    /// Returns a group ID that cancels all lines at once, or null if the target has no AABB.
    /// </summary>
    public string ShowBoundsConnector(Rect2 screenRect, Node3D target, Color color, float width = 2f,
        float duration = 0f, float fadeIn = 0f, float fadeOut = 0f)
    {
        var aabb = OverlayGeometry.ComputeGlobalAabb(target);
        if (!aabb.HasValue) return null;

        var corners3D = OverlayGeometry.GetAabbCorners(aabb.Value);
        var screenCorners = new Vector2[]
        {
            screenRect.Position,
            new(screenRect.End.X, screenRect.Position.Y),
            screenRect.End,
            new(screenRect.Position.X, screenRect.End.Y),
        };

        return ShowLines(screenCorners, corners3D, color, width, duration, fadeIn, fadeOut, autoPair: true);
    }

    /// <summary>
    /// Draw lines connecting pairs of world points (matched by index).
    /// Returns a group ID that cancels all lines at once.
    /// </summary>
    public string ShowLines(Vector3[] fromWorld, Vector3[] toWorld, Color color, float width = 2f, float duration = 0f,
        float fadeIn = 0f, float fadeOut = 0f)
    {
        var groupId = $"linegroup_{_nextId++}";
        int count = Mathf.Min(fromWorld.Length, toWorld.Length);
        for (int i = 0; i < count; i++)
        {
            var line = new Line2D();
            line.Width = width;
            line.DefaultColor = color;
            line.AddPoint(Vector2.Zero);
            line.AddPoint(Vector2.Zero);
            _canvasLayer.AddChild(line);

            _lines.Add(new ActiveLine
            {
                Id = $"line_{_nextId++}",
                GroupId = groupId,
                Instance = line,
                FromWorld = fromWorld[i],
                ToWorld = toWorld[i],
                Color = color,
                Duration = duration,
                FadeIn = fadeIn,
                FadeOut = fadeOut,
            });
        }

        return groupId;
    }

    private void ProcessLines(Camera3D camera, float delta)
    {
        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            var line = _lines[i];
            line.Elapsed += delta;

            if (line.FixedFromScreen.HasValue)
            {
                // Screen-to-world line
                if (camera == null || camera.IsPositionBehind(line.ToWorld)
                                   || camera.GlobalPosition.DistanceSquaredTo(line.ToWorld) < 0.001f)
                {
                    line.Instance.Visible = false;
                }
                else
                {
                    line.Instance.SetPointPosition(0, line.FixedFromScreen.Value);
                    line.Instance.SetPointPosition(1, camera.UnprojectPosition(line.ToWorld));
                    line.Instance.Visible = true;
                }
            }
            else if (camera == null)
            {
                line.Instance.Visible = false;
            }
            else
            {
                // World-to-world line
                bool fromSafe = !camera.IsPositionBehind(line.FromWorld)
                                && camera.GlobalPosition.DistanceSquaredTo(line.FromWorld) > 0.001f;
                bool toSafe = !camera.IsPositionBehind(line.ToWorld)
                              && camera.GlobalPosition.DistanceSquaredTo(line.ToWorld) > 0.001f;

                if (!fromSafe && !toSafe)
                {
                    line.Instance.Visible = false;
                }
                else
                {
                    var fromScreen = fromSafe
                        ? camera.UnprojectPosition(line.FromWorld)
                        : camera.UnprojectPosition(line.ToWorld);
                    var toScreen = toSafe
                        ? camera.UnprojectPosition(line.ToWorld)
                        : camera.UnprojectPosition(line.FromWorld);
                    line.Instance.SetPointPosition(0, fromScreen);
                    line.Instance.SetPointPosition(1, toScreen);
                    line.Instance.Visible = true;
                }
            }

            // Fade
            float lineAlpha = 1f;
            if (line.FadeIn > 0f && line.Elapsed < line.FadeIn)
                lineAlpha = line.Elapsed / line.FadeIn;
            if (line.Duration > 0f && line.FadeOut > 0f)
            {
                float timeLeft = line.Duration - line.Elapsed;
                if (timeLeft < line.FadeOut)
                    lineAlpha = Mathf.Min(lineAlpha, timeLeft / line.FadeOut);
            }

            line.Instance.Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(lineAlpha, 0f, 1f));

            if (line.Duration > 0f && line.Elapsed >= line.Duration)
            {
                RemoveLineAt(i);
            }
        }
    }

    private void RemoveLineAt(int index)
    {
        var line = _lines[index];
        line.Instance.QueueFree();
        _lines.RemoveAt(index);
    }

    private class ActiveLine
    {
        public string Id;
        public string GroupId;
        public Line2D Instance;
        public Vector3 FromWorld;
        public Vector3 ToWorld;
        public Vector2? FixedFromScreen;
        public Color Color;
        public float Duration;
        public float Elapsed;
        public float FadeIn;
        public float FadeOut;
    }
}
