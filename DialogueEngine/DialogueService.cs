using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.DialogueEngine;

public partial class DialogueService : Node
{
    [Export] public RichTextLabel DialogueLabel { get; set; }
    [Export] public RichTextLabel DialogueTitle { get; set; }
    [Export] public Control DialogueContainer { get; set; }
    [Export] public AudioStreamPlayer TypewriterPlayer { get; set; }
    [Export] public AudioStream[] TypewriterSounds { get; set; }
    [Export(PropertyHint.Range, "0.0,0.2")] public float PitchVariation { get; set; } = 0.05f;
    [Export(PropertyHint.Range, "0.0,0.3")] public float SoundCooldown { get; set; } = 0.10f;

    private readonly Queue<DialogueEvent> _queue = new();
    private DialogueEvent _current;
    private float _elapsed;
    private float _duration;
    private float _lingerRemaining;
    private bool _typewriterComplete;
    private bool _lingerComplete;
    private int _lastVisibleChars;
    private float _soundCooldownRemaining;

    private bool IsInterruptible => _current?.Mode switch
    {
        DialogueMode.Narration => false,
        DialogueMode.Dialogue => true,
        DialogueMode.Cutscene => false,
        _ => false
    };

    private bool IsAutoDismiss => _current?.Mode switch
    {
        DialogueMode.Narration => true,
        DialogueMode.Dialogue => false,
        DialogueMode.Cutscene => false,
        _ => false
    };

    public override void _Ready()
    {
        EventBus.EventBus.EventBus.Instance.Subscribe<DialogueEvent>(OnDialogueEvent);
        Hide();
    }

    public override void _Process(double delta)
    {
        if (_current == null) return;

        ProcessTypewriter((float)delta);
        ProcessLinger((float)delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_current == null) return;
        if (!@event.IsPressed() || @event.IsEcho()) return;

        // Only respond to keyboard, gamepad buttons, and mouse clicks
        if (@event is not (InputEventKey or InputEventJoypadButton or InputEventMouseButton)) return;

        HandleInput();
        GetViewport().SetInputAsHandled();
    }

    private void OnDialogueEvent(DialogueEvent e)
    {
        if (e.Override)
        {
            _queue.Clear();
            ShowDialogue(e);
        }
        else if (_current == null)
        {
            ShowDialogue(e);
        }
        else
        {
            _queue.Enqueue(e);
        }
    }

    private void ShowDialogue(DialogueEvent e)
    {
        _current = e;
        _elapsed = 0f;
        _typewriterComplete = false;
        _lingerComplete = false;
        _lastVisibleChars = 0;
        _soundCooldownRemaining = 0f;

        DialogueLabel.Text = e.Content;
        DialogueLabel.VisibleRatio = 0f;

        // Handle title
        if (DialogueTitle != null)
        {
            if (!string.IsNullOrEmpty(e.Title))
            {
                DialogueTitle.Text = e.Title;
                DialogueTitle.Visible = true;

                if (e.TitleColor.HasValue)
                {
                    DialogueTitle.AddThemeColorOverride("default_color", e.TitleColor.Value);
                }
                else
                {
                    DialogueTitle.RemoveThemeColorOverride("default_color");
                }
            }
            else
            {
                DialogueTitle.Visible = false;
            }
        }

        _duration = e.Duration;
        _lingerRemaining = e.LingerDuration;

        Show();
    }

    private void ProcessTypewriter(float delta)
    {
        if (_typewriterComplete) return;

        _elapsed += delta;
        _soundCooldownRemaining -= delta;

        float t = Mathf.Clamp(_elapsed / _duration, 0f, 1f);

        if (_current.TypewriterCurve != null)
        {
            // SampleBaked expects 0-1 normalized input
            DialogueLabel.VisibleRatio = _current.TypewriterCurve.SampleBaked(t);
        }
        else
        {
            // Linear fallback
            DialogueLabel.VisibleRatio = t;
        }

        // Play sound when new characters appear
        int visibleChars = DialogueLabel.VisibleCharacters;
        if (visibleChars > _lastVisibleChars)
        {
            TryPlayTypewriterSound();
            _lastVisibleChars = visibleChars;
        }

        if (t >= 1f)
        {
            _typewriterComplete = true;
        }
    }

    private void TryPlayTypewriterSound()
    {
        if (TypewriterPlayer == null || TypewriterSounds == null || TypewriterSounds.Length == 0)
            return;

        // Cooldown prevents clicks from stacking, but tails can overlap
        if (_soundCooldownRemaining > 0f)
            return;

        _soundCooldownRemaining = SoundCooldown;

        var sound = TypewriterSounds[GD.RandRange(0, TypewriterSounds.Length - 1)];
        TypewriterPlayer.Stream = sound;
        TypewriterPlayer.PitchScale = (float)GD.RandRange(1f - PitchVariation, 1f + PitchVariation);
        TypewriterPlayer.Play();
    }

    private void ProcessLinger(float delta)
    {
        if (!_typewriterComplete || _lingerComplete) return;

        _lingerRemaining -= delta;
        if (_lingerRemaining <= 0f)
        {
            _lingerComplete = true;
            OnLingerComplete();
        }
    }

    private void HandleInput()
    {
        if (!_typewriterComplete && IsInterruptible)
        {
            // Skip to end of typewriter
            DialogueLabel.VisibleRatio = 1f;
            _typewriterComplete = true;
        }
        else if (_lingerComplete && !IsAutoDismiss)
        {
            // Dismiss dialogue
            Dismiss();
        }
    }

    private void OnLingerComplete()
    {
        if (IsAutoDismiss)
        {
            Dismiss();
        }
    }

    private void Dismiss()
    {
        _current = null;
        Hide();

        if (_queue.Count > 0)
        {
            ShowDialogue(_queue.Dequeue());
        }
    }

    private void Show()
    {
        DialogueContainer?.Show();
    }

    private void Hide()
    {
        DialogueContainer?.Hide();
    }
}