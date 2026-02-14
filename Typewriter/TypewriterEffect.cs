using Godot;

namespace GodotFeatureLibrary.Typewriter;

/// <summary>
/// Reusable typewriter effect for RichTextLabels.
/// Add as a child node and call Update() from _Process().
/// </summary>
public partial class TypewriterEffect : Node
{
    [Export] public AudioStream[] Sounds { get; set; } = [];
    [Export(PropertyHint.Range, "0.0,0.2")] public float PitchVariation { get; set; } = 0.05f;
    [Export(PropertyHint.Range, "0.0,0.3")] public float SoundCooldown { get; set; } = 0.08f;
    [Export] public float FastForwardMultiplier { get; set; } = 4f;

    public bool IsActive { get; private set; }
    public bool IsComplete { get; private set; }

    private RichTextLabel _label;
    private AudioStreamPlayer _audioPlayer;
    private RandomNumberGenerator _rng = new();

    private float _duration;
    private Curve _curve;
    private float _elapsed;
    private int _lastVisibleChars;
    private float _soundCooldownRemaining;
    private bool _fastForward;

    public override void _Ready()
    {
        _audioPlayer = new AudioStreamPlayer();
        AddChild(_audioPlayer);
    }

    /// <summary>
    /// Start the typewriter effect on a label.
    /// </summary>
    public void Start(RichTextLabel label, float duration, Curve curve = null)
    {
        _label = label;
        _duration = duration;
        _curve = curve;
        _elapsed = 0f;
        _lastVisibleChars = 0;
        _soundCooldownRemaining = 0f;
        _fastForward = false;
        IsActive = true;
        IsComplete = false;

        if (_label != null)
        {
            _label.VisibleRatio = 0f;
        }
    }

    /// <summary>
    /// Call this from the parent's _Process().
    /// </summary>
    public void Update(float delta)
    {
        if (!IsActive || _label == null) return;

        float effectiveDelta = _fastForward ? delta * FastForwardMultiplier : delta;

        _soundCooldownRemaining -= delta; // Real delta for sound cooldown
        _elapsed += effectiveDelta;

        float t = Mathf.Clamp(_elapsed / _duration, 0f, 1f);

        // Apply curve if provided, otherwise linear
        if (_curve != null)
        {
            _label.VisibleRatio = _curve.SampleBaked(t);
        }
        else
        {
            _label.VisibleRatio = t;
        }

        // Play sound when new characters appear
        int visibleChars = _label.VisibleCharacters;
        if (visibleChars > _lastVisibleChars)
        {
            TryPlaySound();
            _lastVisibleChars = visibleChars;
        }

        if (t >= 1f)
        {
            IsActive = false;
            IsComplete = true;
            _fastForward = false;
        }
    }

    /// <summary>
    /// Enable fast-forward mode (multiplied speed).
    /// </summary>
    public void SetFastForward(bool enabled)
    {
        _fastForward = enabled;
    }

    /// <summary>
    /// Immediately complete the typewriter effect.
    /// </summary>
    public void Complete()
    {
        if (_label != null)
        {
            _label.VisibleRatio = 1f;
        }
        IsActive = false;
        IsComplete = true;
        _fastForward = false;
    }

    /// <summary>
    /// Stop the typewriter without completing.
    /// </summary>
    public void Stop()
    {
        IsActive = false;
        _fastForward = false;
        _label = null;
    }

    private void TryPlaySound()
    {
        if (_audioPlayer == null || Sounds == null || Sounds.Length == 0)
            return;

        if (_soundCooldownRemaining > 0) return;

        _soundCooldownRemaining = SoundCooldown;

        var sound = Sounds[_rng.RandiRange(0, Sounds.Length - 1)];
        _audioPlayer.Stream = sound;
        _audioPlayer.PitchScale = (float)GD.RandRange(1f - PitchVariation, 1f + PitchVariation);
        _audioPlayer.Play();
    }
}
