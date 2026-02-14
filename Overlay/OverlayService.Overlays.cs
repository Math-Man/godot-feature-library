using Godot;

namespace GodotFeatureLibrary.Overlay;

public partial class OverlayService
{
    /// <summary>
    /// Show an overlay at a fixed screen position.
    /// </summary>
    public string Show(PackedScene scene, Vector2 screenPosition, float duration, float fadeIn = 0f, float fadeOut = 0f)
    {
        var instance = scene.Instantiate<Control>();
        instance.Position = screenPosition;
        if (fadeIn > 0f) instance.Modulate = new Color(1f, 1f, 1f, 0f);
        _container.AddChild(instance);

        var id = $"overlay_{_nextId++}";
        _overlays.Add(new ActiveOverlay
        {
            Id = id,
            Instance = instance,
            Duration = duration,
            FadeIn = fadeIn,
            FadeOut = fadeOut,
        });

        return id;
    }

    /// <summary>
    /// Show an overlay anchored to a world object. Tracks it each frame.
    /// MaxDistance &lt;= 0 means no distance limit.
    /// </summary>
    public string ShowAnchored(PackedScene scene, Node3D target, Vector2 offset, float duration, float maxDistance = 0f,
        float fadeIn = 0f, float fadeOut = 0f)
    {
        var instance = scene.Instantiate<Control>();
        if (fadeIn > 0f) instance.Modulate = new Color(1f, 1f, 1f, 0f);
        _container.AddChild(instance);

        // Position immediately if possible
        var camera = GetViewport().GetCamera3D();
        if (camera != null && !camera.IsPositionBehind(target.GlobalPosition))
        {
            instance.Position = camera.UnprojectPosition(target.GlobalPosition) + offset;
        }

        var id = $"overlay_{_nextId++}";
        _overlays.Add(new ActiveOverlay
        {
            Id = id,
            Instance = instance,
            Duration = duration,
            Target = target,
            Offset = offset,
            MaxDistance = maxDistance,
            FadeIn = fadeIn,
            FadeOut = fadeOut,
            IsWorldAnchored = true,
        });

        return id;
    }

    /// <summary>
    /// Show an overlay that tracks the bounding box of a world object.
    /// The Control is resized each frame to fit the projected AABB.
    /// Duration &lt;= 0 means infinite (must be cancelled manually).
    /// MaxDistance &lt;= 0 means no distance limit.
    /// </summary>
    public string ShowBounds(PackedScene scene, Node3D target, Vector2 padding = default, float duration = 0f,
        float maxDistance = 0f, float fadeIn = 0f, float fadeOut = 0f)
    {
        var instance = scene.Instantiate<Control>();
        if (fadeIn > 0f) instance.Modulate = new Color(1f, 1f, 1f, 0f);
        _container.AddChild(instance);

        var id = $"overlay_{_nextId++}";
        _overlays.Add(new ActiveOverlay
        {
            Id = id,
            Instance = instance,
            Duration = duration,
            Target = target,
            Offset = padding,
            MaxDistance = maxDistance,
            FadeIn = fadeIn,
            FadeOut = fadeOut,
            IsBoundsTracked = true,
        });

        return id;
    }

    private void ProcessOverlays(Camera3D camera, float delta)
    {
        for (int i = _overlays.Count - 1; i >= 0; i--)
        {
            var overlay = _overlays[i];

            overlay.Elapsed += delta;

            // Distance-based fade for world-space overlays
            float distanceAlpha = 1f;
            if (overlay.MaxDistance > 0f && overlay.Target != null && camera != null)
            {
                var distance = camera.GlobalPosition.DistanceTo(overlay.Target.GlobalPosition);
                if (distance > overlay.MaxDistance)
                {
                    overlay.Instance.Visible = false;
                    continue;
                }

                // Fade starts at 70% of max distance
                float fadeStart = overlay.MaxDistance * 0.7f;
                if (distance > fadeStart)
                    distanceAlpha = 1f - (distance - fadeStart) / (overlay.MaxDistance - fadeStart);
            }

            if (overlay.IsBoundsTracked)
            {
                if (!IsInstanceValid(overlay.Target) || camera == null)
                {
                    RemoveOverlayAt(i);
                    continue;
                }

                var rect = ProjectBounds(camera, overlay.Target, overlay.Offset);
                if (rect.HasValue)
                {
                    overlay.Instance.Position = rect.Value.Position;
                    overlay.Instance.Size = rect.Value.Size;
                    overlay.Instance.Visible = true;
                }
                else
                {
                    overlay.Instance.Visible = false;
                }
            }
            else if (overlay.IsWorldAnchored)
            {
                if (!IsInstanceValid(overlay.Target) || camera == null)
                {
                    RemoveOverlayAt(i);
                    continue;
                }

                if (camera.IsPositionBehind(overlay.Target.GlobalPosition))
                {
                    overlay.Instance.Visible = false;
                }
                else
                {
                    var screenPos = camera.UnprojectPosition(overlay.Target.GlobalPosition);
                    overlay.Instance.Position = screenPos + overlay.Offset;
                    overlay.Instance.Visible = true;
                }
            }

            // Fade in/out via modulate alpha
            float alpha = 1f;
            if (overlay.FadeIn > 0f && overlay.Elapsed < overlay.FadeIn)
                alpha = overlay.Elapsed / overlay.FadeIn;
            if (overlay.Duration > 0f && overlay.FadeOut > 0f)
            {
                float timeLeft = overlay.Duration - overlay.Elapsed;
                if (timeLeft < overlay.FadeOut)
                    alpha = Mathf.Min(alpha, timeLeft / overlay.FadeOut);
            }

            overlay.Instance.Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(alpha * distanceAlpha, 0f, 1f));

            if (overlay.Duration > 0f && overlay.Elapsed >= overlay.Duration)
            {
                RemoveOverlayAt(i);
            }
        }
    }

    private void RemoveOverlayAt(int index)
    {
        var overlay = _overlays[index];
        overlay.Instance.QueueFree();
        _overlays.RemoveAt(index);
    }

    private Rect2? ProjectBounds(Camera3D camera, Node3D target, Vector2 padding)
    {
        var aabb = OverlayGeometry.ComputeGlobalAabb(target);
        if (!aabb.HasValue) return null;

        var corners = OverlayGeometry.GetAabbCorners(aabb.Value);
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);

        foreach (var corner in corners)
        {
            if (camera.IsPositionBehind(corner))
                return null;

            var screenPos = camera.UnprojectPosition(corner);
            min.X = Mathf.Min(min.X, screenPos.X);
            min.Y = Mathf.Min(min.Y, screenPos.Y);
            max.X = Mathf.Max(max.X, screenPos.X);
            max.Y = Mathf.Max(max.Y, screenPos.Y);
        }

        return new Rect2(
            min - padding,
            max - min + padding * 2f
        );
    }

    private class ActiveOverlay
    {
        public string Id;
        public Control Instance;
        public float Duration;
        public float Elapsed;
        public Node3D Target;
        public Vector2 Offset;
        public float MaxDistance;
        public float FadeIn;
        public float FadeOut;
        public bool IsWorldAnchored;
        public bool IsBoundsTracked;
    }
}
