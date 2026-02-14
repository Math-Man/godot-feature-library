using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.PostProcess;

public class ShaderOverride
{
    public bool? Visible { get; set; }
    public Dictionary<StringName, Variant> Parameters { get; set; } = new();
}

public class PostProcessEffect
{
    public string Id { get; set; }
    public int Priority { get; set; }
    public float TransitionDuration { get; set; } = 0.3f;
    public Dictionary<string, ShaderOverride> Overrides { get; set; } = new();

    // --- Builder ---

    public static Builder Create(string id) => new(id);

    public class Builder
    {
        private readonly PostProcessEffect _effect;
        private string _currentShader;

        internal Builder(string id)
        {
            _effect = new PostProcessEffect { Id = id };
        }

        public Builder WithPriority(int priority)
        {
            _effect.Priority = priority;
            return this;
        }

        public Builder WithTransition(float duration)
        {
            _effect.TransitionDuration = duration;
            return this;
        }

        public Builder Shader(string shaderName, bool? visible = null, params (string param, Variant value)[] parameters)
        {
            var shaderOverride = new ShaderOverride { Visible = visible };
            foreach (var (param, value) in parameters)
            {
                shaderOverride.Parameters[param] = value;
            }
            _effect.Overrides[shaderName] = shaderOverride;
            _currentShader = shaderName;
            return this;
        }

        public Builder Set(string param, Variant value)
        {
            if (_currentShader != null && _effect.Overrides.TryGetValue(_currentShader, out var shaderOverride))
            {
                shaderOverride.Parameters[param] = value;
            }
            return this;
        }

        public PostProcessEffect Build() => _effect;
    }
}
