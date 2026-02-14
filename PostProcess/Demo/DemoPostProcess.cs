using Godot;
using GodotFeatureLibrary.GameInput;

namespace GodotFeatureLibrary.PostProcess.Demo;

public partial class DemoPostProcess : Node
{
    private bool _tintActive;
    private bool _vignetteActive;

    private readonly PostProcessEffect _tintEffect = PostProcessEffect
        .Create("demo-tint")
        .WithTransition(0.5f)
        .Shader("Tint", visible: true)
            .Set("intensity", 0.4f)
            .Set("tint_color", new Color(1f, 0.2f, 0.1f))
        .Build();

    private readonly PostProcessEffect _vignetteEffect = PostProcessEffect
        .Create("demo-vignette")
        .WithTransition(0.8f)
        .Shader("Vignette", visible: true)
            .Set("intensity", 0.9f)
            .Set("radius", 0.45f)
            .Set("softness", 0.4f)
        .Build();

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputMapping.INTERACTION_PRIMARY))
        {
            _tintActive = !_tintActive;
            if (_tintActive)
                PostProcessManager.Instance.Apply(_tintEffect);
            else
                PostProcessManager.Instance.Remove("demo-tint");
        }

        if (Input.IsActionJustPressed(InputMapping.INTERACTION_SECONDARY))
        {
            _vignetteActive = !_vignetteActive;
            if (_vignetteActive)
                PostProcessManager.Instance.Apply(_vignetteEffect);
            else
                PostProcessManager.Instance.Remove("demo-vignette");
        }
    }
}
