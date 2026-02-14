using System.Collections.Generic;
using Godot;
using GodotFeatureLibrary.Events;
using GodotFeatureLibrary.Typewriter;

namespace GodotFeatureLibrary.DialogueEngine;

public partial class DialogueService : Node
{
    [Export] public RichTextLabel DialogueLabel { get; set; }
    [Export] public RichTextLabel DialogueTitle { get; set; }
    [Export] public Control DialogueContainer { get; set; }
    [Export] public AudioStream[] TypewriterSounds { get; set; }
    [Export(PropertyHint.Range, "0.0,0.2")] public float PitchVariation { get; set; } = 0.05f;
    [Export(PropertyHint.Range, "0.0,0.3")] public float SoundCooldown { get; set; } = 0.10f;
    [Export] public float FastForwardMultiplier { get; set; } = 5f;

    private TypewriterEffect _typewriter;
    private readonly Queue<DialogueEvent> _queue = new();
    private DialogueEvent _current;
    private float _duration;
    private float _lingerRemaining;
    private bool _lingerComplete;
    private bool _fastForward;

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

    private bool ShouldLockControls => _current?.Mode switch
    {
        DialogueMode.Narration => false,
        DialogueMode.Dialogue => true,
        DialogueMode.Cutscene => true,
        _ => false
    };

    public override void _Ready()
    {
        _typewriter = new TypewriterEffect
        {
            Sounds = TypewriterSounds,
            PitchVariation = PitchVariation,
            SoundCooldown = SoundCooldown,
            FastForwardMultiplier = FastForwardMultiplier
        };
        AddChild(_typewriter);

        EventBus.Instance.Subscribe<DialogueEvent>(OnDialogueEvent);
        Hide();
    }

    public override void _Process(double delta)
    {
        if (_current == null) return;

        _typewriter.Update((float)delta);
        ProcessLinger((float)delta);
    }

    // Mouse input uses _Input (catches events BEFORE UI consumes them)
    // This ensures clicking anywhere advances dialogue, even over UI elements
    public override void _Input(InputEvent @event)
    {
        if (_current == null) return;
        if (@event is not InputEventMouseButton) return;
        if (!@event.IsPressed()) return;

        HandleInput();
        GetViewport().SetInputAsHandled();
    }

    // Keyboard/gamepad uses _UnhandledInput (catches events AFTER other nodes)
    // This lets menus and other systems get first dibs on input
    // Dialogue only advances if nothing else consumed the event
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_current == null) return;
        if (!@event.IsPressed() || @event.IsEcho()) return;

        if (@event is not (InputEventKey or InputEventJoypadButton)) return;

        HandleInput();
        GetViewport().SetInputAsHandled();
    }

    private void OnDialogueEvent(DialogueEvent e)
    {
        if (e.Override)
        {
            _queue.Clear();
            if (_current != null && ShouldLockControls)
            {
                EventBus.Instance?.Publish(new DialogueDismissedEvent(_current.Mode));
            }
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
        _lingerComplete = false;
        _fastForward = false;

        if (ShouldLockControls)
        {
            EventBus.Instance?.Publish(new DialogueStartedEvent(e.Mode));
        }

        DialogueLabel.Text = e.Content;

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

        _typewriter.Start(DialogueLabel, _duration, e.TypewriterCurve);
        Show();
    }

    private void ProcessLinger(float delta)
    {
        if (!_typewriter.IsComplete || _lingerComplete) return;

        float effectiveDelta = _fastForward ? delta * FastForwardMultiplier : delta;
        _lingerRemaining -= effectiveDelta;
        if (_lingerRemaining <= 0f)
        {
            _lingerComplete = true;
            OnLingerComplete();
        }
    }

    private void HandleInput()
    {
        if (!_typewriter.IsComplete && IsInterruptible)
        {
            // Speed up typewriter and linger
            _fastForward = true;
            _typewriter.SetFastForward(true);
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
        if (ShouldLockControls)
        {
            EventBus.Instance?.Publish(new DialogueDismissedEvent(_current.Mode));
        }

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
