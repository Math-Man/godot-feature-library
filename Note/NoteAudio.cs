using Godot;

namespace GodotFeatureLibrary.Note;

/// <summary>
/// Handles audio playback for the Note system.
/// </summary>
public partial class NoteAudio : Node
{
    [Export] public AudioStream OpenSound { get; set; }
    [Export] public AudioStream CloseSound { get; set; }
    [Export] public AudioStream[] PageTurnSounds { get; set; } = [];

    private AudioStreamPlayer _player;
    private RandomNumberGenerator _rng = new();

    // Active sounds (can be overridden per-note)
    private AudioStream _activeOpenSound;
    private AudioStream _activeCloseSound;
    private AudioStream[] _activePageTurnSounds;

    public override void _Ready()
    {
        _player = new AudioStreamPlayer();
        AddChild(_player);
    }

    /// <summary>
    /// Set active sounds for current note (uses defaults if null).
    /// </summary>
    public void SetActiveSounds(
        AudioStream openOverride = null,
        AudioStream closeOverride = null,
        AudioStream[] pageTurnOverride = null)
    {
        _activeOpenSound = openOverride ?? OpenSound;
        _activeCloseSound = closeOverride ?? CloseSound;
        _activePageTurnSounds = pageTurnOverride is { Length: > 0 } ? pageTurnOverride : PageTurnSounds;
    }

    public void PlayOpen()
    {
        PlaySound(_activeOpenSound);
    }

    public void PlayClose()
    {
        PlaySound(_activeCloseSound);
    }

    public void PlayPageTurn()
    {
        PlayRandomSound(_activePageTurnSounds);
    }

    private void PlaySound(AudioStream sound)
    {
        if (sound == null || _player == null) return;
        if (_player.Playing) return;
        _player.Stream = sound;
        _player.Play();
    }

    private void PlayRandomSound(AudioStream[] sounds)
    {
        if (sounds == null || sounds.Length == 0) return;
        var sound = sounds[_rng.RandiRange(0, sounds.Length - 1)];
        PlaySound(sound);
    }
}
