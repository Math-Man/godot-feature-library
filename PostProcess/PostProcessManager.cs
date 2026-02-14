using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotFeatureLibrary.PostProcess;

public partial class PostProcessManager : Node
{
    public static PostProcessManager Instance { get; private set; }

    [Export] public NodePath EffectsContainerPath { get; set; }

    private Control _effectsContainer;
    private readonly Dictionary<string, ShaderEntry> _shaders = new();
    private readonly Dictionary<string, ActiveEffect> _effects = new();
    private HashSet<string> _dirtyShaders = new();

    public override void _Ready()
    {
        Instance = this;
        if (EffectsContainerPath != null && !EffectsContainerPath.IsEmpty)
            _effectsContainer = GetNode<Control>(EffectsContainerPath);
        DiscoverShaders();
    }

    public override void _Process(double delta)
    {
        if (_effects.Count == 0) return;

        float dt = (float)delta;
        List<string> completed = null;

        // Update transitions
        foreach (var (id, active) in _effects)
        {
            if (active.Removing)
            {
                float rate = active.FadeOutDuration > 0 ? dt / active.FadeOutDuration : 1f;
                active.Progress = Mathf.Max(0f, active.Progress - rate);
                if (active.Progress <= 0f)
                {
                    completed ??= new();
                    completed.Add(id);
                }
            }
            else
            {
                float rate = active.Effect.TransitionDuration > 0
                    ? dt / active.Effect.TransitionDuration
                    : 1f;
                active.Progress = Mathf.Min(1f, active.Progress + rate);
            }
        }

        // Remove fully faded effects
        if (completed != null)
        {
            foreach (var id in completed)
            {
                MarkDirty(_effects[id]);
                _effects.Remove(id);
            }
        }

        RecalculateShaders();
    }

    public void Apply(PostProcessEffect effect)
    {
        if (string.IsNullOrEmpty(effect?.Id)) return;

        // Replace existing effect with same ID
        if (_effects.TryGetValue(effect.Id, out var existing))
        {
            MarkDirty(existing);
            _effects.Remove(effect.Id);
        }

        _effects[effect.Id] = new ActiveEffect
        {
            Effect = effect,
            Progress = 0f,
            Removing = false,
            FadeOutDuration = effect.TransitionDuration
        };
    }

    public void Remove(string effectId, float? fadeDuration = null)
    {
        if (!_effects.TryGetValue(effectId, out var active)) return;
        if (active.Removing) return;

        active.Removing = true;
        active.FadeOutDuration = fadeDuration ?? active.Effect.TransitionDuration;
    }

    public void RemoveInstant(string effectId)
    {
        if (_effects.TryGetValue(effectId, out var active))
        {
            MarkDirty(active);
            _effects.Remove(effectId);
            RecalculateShaders();
        }
    }

    public void RemoveDomain(string domain, float? fadeDuration = null)
    {
        foreach (var (id, active) in _effects)
        {
            if (id.StartsWith(domain) && !active.Removing)
            {
                active.Removing = true;
                active.FadeOutDuration = fadeDuration ?? active.Effect.TransitionDuration;
            }
        }
    }

    public bool HasEffect(string effectId) =>
        _effects.ContainsKey(effectId) && !_effects[effectId].Removing;

    public bool HasDomain(string domain) =>
        _effects.Any(kv => kv.Key.StartsWith(domain) && !kv.Value.Removing);

    // --- Internals ---

    private void DiscoverShaders()
    {
        if (_effectsContainer == null)
        {
            GD.PushWarning("PostProcessManager: EffectsContainer not assigned.");
            return;
        }

        var viewportSize = GetViewport().GetVisibleRect().Size;

        foreach (var child in _effectsContainer.GetChildren())
        {
            if (child is not BackBufferCopy bbc) continue;

            foreach (var bbcChild in bbc.GetChildren())
            {
                if (bbcChild is not ColorRect colorRect) continue;
                if (colorRect.Material is not ShaderMaterial material) continue;

                // Ensure ColorRect covers full viewport (anchors don't work under Node2D)
                colorRect.Position = Vector2.Zero;
                colorRect.Size = viewportSize;

                string name = colorRect.Name;
                if (_shaders.ContainsKey(name))
                {
                    GD.PushWarning($"PostProcessManager: Duplicate shader name '{name}'. Rename one of them.");
                    continue;
                }

                _shaders[name] = new ShaderEntry
                {
                    Node = colorRect,
                    Material = material,
                    BaseVisible = colorRect.Visible,
                    BaseParams = new Dictionary<StringName, Variant>()
                };

                GD.Print($"PostProcessManager: Registered shader '{name}'.");
            }
        }
    }

    private void EnsureBaseParam(ShaderEntry entry, StringName param)
    {
        if (!entry.BaseParams.ContainsKey(param))
        {
            entry.BaseParams[param] = entry.Material.GetShaderParameter(param);
        }
    }

    private void MarkDirty(ActiveEffect active)
    {
        foreach (var shaderName in active.Effect.Overrides.Keys)
        {
            _dirtyShaders.Add(shaderName);
        }
    }

    private void RecalculateShaders()
    {
        // Collect all shaders targeted by active effects
        var affected = new HashSet<string>();
        foreach (var (_, active) in _effects)
        {
            foreach (var shaderName in active.Effect.Overrides.Keys)
            {
                affected.Add(shaderName);
            }
        }

        // Also recalculate shaders that were dirty (had effects removed)
        affected.UnionWith(_dirtyShaders);
        _dirtyShaders.Clear();

        foreach (var shaderName in affected)
        {
            if (!_shaders.TryGetValue(shaderName, out var entry)) continue;

            // Collect applicable effects sorted by priority (ascending)
            var applicable = new List<(ActiveEffect Active, ShaderOverride Override)>();
            foreach (var (_, active) in _effects)
            {
                if (active.Effect.Overrides.TryGetValue(shaderName, out var over))
                {
                    applicable.Add((active, over));
                }
            }

            // No active effects targeting this shader - restore to base
            if (applicable.Count == 0)
            {
                RestoreToBase(entry);
                continue;
            }

            applicable.Sort((a, b) => a.Active.Effect.Priority.CompareTo(b.Active.Effect.Priority));

            // Resolve visibility
            bool visible = entry.BaseVisible;
            foreach (var (active, over) in applicable)
            {
                if (over.Visible.HasValue && active.Progress > 0.01f)
                {
                    visible = over.Visible.Value;
                }
            }
            entry.Node.Visible = visible;

            // Resolve parameters
            var touchedParams = new HashSet<StringName>();
            foreach (var (_, over) in applicable)
            {
                foreach (var param in over.Parameters.Keys)
                    touchedParams.Add(param);
            }

            foreach (var param in touchedParams)
            {
                EnsureBaseParam(entry, param);
                Variant current = entry.BaseParams[param];

                foreach (var (active, over) in applicable)
                {
                    if (over.Parameters.TryGetValue(param, out var target))
                    {
                        current = LerpVariant(current, target, active.Progress);
                    }
                }

                entry.Material.SetShaderParameter(param, current);
            }
        }
    }

    private void RestoreToBase(ShaderEntry entry)
    {
        entry.Node.Visible = entry.BaseVisible;
        foreach (var (param, value) in entry.BaseParams)
        {
            entry.Material.SetShaderParameter(param, value);
        }
    }

    private static Variant LerpVariant(Variant from, Variant to, float t)
    {
        if (t >= 1f) return to;
        if (t <= 0f) return from;

        return (from.VariantType, to.VariantType) switch
        {
            (Variant.Type.Float, Variant.Type.Float) =>
                Mathf.Lerp((float)from, (float)to, t),

            (Variant.Type.Int, Variant.Type.Int) =>
                (int)Mathf.Lerp((int)from, (int)to, t),

            (Variant.Type.Vector2, Variant.Type.Vector2) =>
                ((Vector2)from).Lerp((Vector2)to, t),

            (Variant.Type.Vector3, Variant.Type.Vector3) =>
                ((Vector3)from).Lerp((Vector3)to, t),

            (Variant.Type.Vector4, Variant.Type.Vector4) =>
                ((Vector4)from).Lerp((Vector4)to, t),

            (Variant.Type.Color, Variant.Type.Color) =>
                ((Color)from).Lerp((Color)to, t),

            (Variant.Type.Bool, Variant.Type.Bool) =>
                t > 0.5f ? to : from,

            _ => t > 0.5f ? to : from
        };
    }

    // --- Internal types ---

    private class ShaderEntry
    {
        public ColorRect Node;
        public ShaderMaterial Material;
        public bool BaseVisible;
        public Dictionary<StringName, Variant> BaseParams;
    }

    private class ActiveEffect
    {
        public PostProcessEffect Effect;
        public float Progress;
        public bool Removing;
        public float FadeOutDuration;
    }
}
