using System.Collections.Generic;
using Godot;
using GodotFeatureLibrary.GameInput;
using GodotFeatureLibrary.Typewriter;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Note;

// Events for other systems to react to
public class NoteOpenedEvent
{
    public NoteData Note { get; }
    public NoteOpenedEvent(NoteData note) => Note = note;
}

public class NoteClosedEvent
{
    public NoteData Note { get; }
    public NoteClosedEvent(NoteData note) => Note = note;
}

public partial class NoteService : Node
{
    public static NoteService Instance { get; private set; }

    [Export] public PackedScene NoteLayoutScene { get; set; }
    [Export] public bool PauseOnOpen { get; set; } = false;

    [ExportGroup("Transitions")]
    [Export] public float OpenCloseDuration { get; set; } = 0.3f;
    [Export] public float PageTransitionDuration { get; set; } = 0.2f;

    [ExportGroup("Audio")]
    [Export] public AudioStream OpenSound { get; set; }
    [Export] public AudioStream CloseSound { get; set; }
    [Export] public AudioStream[] PageTurnSounds { get; set; } = [];

    [ExportGroup("Typewriter")]
    [Export] public bool UseTypewriter { get; set; } = false;
    [Export] public float TypewriterDuration { get; set; } = 2f;
    [Export] public AudioStream[] TypewriterSounds { get; set; } = [];
    [Export(PropertyHint.Range, "0.0,0.2")] public float TypewriterPitchVariation { get; set; } = 0.05f;
    [Export(PropertyHint.Range, "0.0,0.3")] public float TypewriterSoundCooldown { get; set; } = 0.08f;

    private NoteAudio _audio;
    private TypewriterEffect _typewriter;
    private Control _currentLayout;
    private NoteData _currentNote;
    private int _currentPage;
    private HashSet<string> _collectedNoteIds = new();
    private Tween _pageTween;
    private Tween _openCloseTween;
    private bool _isTransitioning;
    private bool _isPageTransitioning;
    private int _highestPageVisited;
    private bool _activeUseTypewriter;

    // UI references (set after instantiation)
    private TextureRect _imageRect;
    private Control _blurOverlay;
    private RichTextLabel _transcriptLabel;
    private Label _pageLabel;
    private Label _leftArrow;
    private Label _rightArrow;

    public bool IsOpen => _currentLayout != null && _currentLayout.Visible && !_isTransitioning;
    public IReadOnlySet<string> CollectedNotes => _collectedNoteIds;

    public override void _Ready()
    {
        Instance = this;

        _audio = new NoteAudio
        {
            OpenSound = OpenSound,
            CloseSound = CloseSound,
            PageTurnSounds = PageTurnSounds
        };
        AddChild(_audio);

        _typewriter = new TypewriterEffect
        {
            Sounds = TypewriterSounds,
            PitchVariation = TypewriterPitchVariation,
            SoundCooldown = TypewriterSoundCooldown
        };
        AddChild(_typewriter);
    }

    public override void _Process(double delta)
    {
        _typewriter.Update((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsOpen) return;

        if (@event.IsActionPressed(InputMapping.INTERACTION_PRIMARY))
        {
            if (_isPageTransitioning) { /* ignore during transition */ }
            else if (_typewriter.IsActive)
            {
                _typewriter.SetFastForward(true);
            }
            else
            {
                NextPage();
            }
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionReleased(InputMapping.INTERACTION_PRIMARY))
        {
            if (_typewriter.IsActive)
            {
                _typewriter.SetFastForward(false);
            }
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed(InputMapping.INTERACTION_SECONDARY))
        {
            if (!_isPageTransitioning)
                PreviousPage();
            GetViewport().SetInputAsHandled();
        }
    }


    public void OpenNote(
        NoteData data,
        PackedScene layoutOverride = null,
        AudioStream openSoundOverride = null,
        AudioStream closeSoundOverride = null,
        AudioStream[] pageTurnSoundsOverride = null,
        bool? useTypewriterOverride = null)
    {
        if (_isTransitioning || (_currentLayout != null && _currentLayout.Visible) || data == null) return;

        _currentNote = data;
        _currentPage = 0;
        _highestPageVisited = 0;

        // Set active settings (override or default)
        _audio.SetActiveSounds(openSoundOverride, closeSoundOverride, pageTurnSoundsOverride);
        _activeUseTypewriter = useTypewriterOverride ?? UseTypewriter;

        // Mark as collected
        if (!string.IsNullOrEmpty(data.Id))
        {
            _collectedNoteIds.Add(data.Id);
        }

        // Determine which layout to use
        var layoutScene = layoutOverride ?? NoteLayoutScene;
        if (layoutScene == null) return;

        // Create UI (recreate each time to support different layouts)
        _currentLayout?.QueueFree();
        _currentLayout = layoutScene.Instantiate<Control>();
        GetTree().Root.AddChild(_currentLayout);
        CacheUIReferences();

        UpdateDisplay(instant: true);
        TransitionOpen();

        if (PauseOnOpen)
        {
            GetTree().Paused = true;
        }

        EventBus.Instance?.Publish(new NoteOpenedEvent(data));
    }

    public void CloseNote()
    {
        if (_isTransitioning || _currentLayout == null || !_currentLayout.Visible) return;

        TransitionClose();
    }

    private void TransitionOpen()
    {
        _isTransitioning = true;
        _currentLayout.Modulate = new Color(1, 1, 1, 0);
        _currentLayout.Visible = true;

        _audio.PlayOpen();

        _openCloseTween?.Kill();
        _openCloseTween = CreateTween();
        _openCloseTween.TweenProperty(_currentLayout, "modulate:a", 1.0f, OpenCloseDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _openCloseTween.TweenCallback(Callable.From(() => _isTransitioning = false));
    }

    private void TransitionClose()
    {
        _isTransitioning = true;
        var closedNote = _currentNote;

        _typewriter.Stop();
        _audio.PlayClose();

        _openCloseTween?.Kill();
        _openCloseTween = CreateTween();
        _openCloseTween.TweenProperty(_currentLayout, "modulate:a", 0.0f, OpenCloseDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _openCloseTween.TweenCallback(Callable.From(() =>
        {
            _currentLayout.Visible = false;
            _isTransitioning = false;

            if (PauseOnOpen)
            {
                GetTree().Paused = false;
            }

            _currentNote = null;
            _currentPage = 0;

            EventBus.Instance?.Publish(new NoteClosedEvent(closedNote));
        }));
    }

    public void NextPage()
    {
        if (_currentNote == null) return;

        if (_currentPage >= _currentNote.TotalPages - 1)
        {
            // On last page, close
            CloseNote();
        }
        else
        {
            _currentPage++;
            _audio.PlayPageTurn();
            UpdateDisplay();
        }
    }

    public void PreviousPage()
    {
        if (_currentNote == null) return;

        if (_currentPage <= 0)
        {
            CloseNote();
        }
        else
        {
            _audio.PlayPageTurn();
            _currentPage--;
            UpdateDisplay();
        }
    }

    public bool HasCollected(string noteId) => _collectedNoteIds.Contains(noteId);

    public IEnumerable<string> GetCollectedNoteIds() => _collectedNoteIds;

    public void LoadCollectedNotes(IEnumerable<string> ids)
    {
        _collectedNoteIds.Clear();
        foreach (var id in ids)
        {
            _collectedNoteIds.Add(id);
        }
    }

    private void CacheUIReferences()
    {
        // These paths assume a specific scene structure - adjust as needed
        _imageRect = _currentLayout.GetNodeOrNull<TextureRect>("NoteImage");
        _blurOverlay = _currentLayout.GetNodeOrNull<Control>("BlurOverlay");
        _transcriptLabel = _currentLayout.GetNodeOrNull<RichTextLabel>("TranscriptLabel");
        _pageLabel = _currentLayout.GetNodeOrNull<Label>("PageLabel");
        _leftArrow = _currentLayout.GetNodeOrNull<Label>("LeftArrow");
        _rightArrow = _currentLayout.GetNodeOrNull<Label>("RightArrow");
    }

    private void UpdateDisplay(bool instant = false)
    {
        if (_currentNote == null) return;

        // Image
        if (_imageRect != null)
        {
            _imageRect.Texture = _currentNote.Image;
        }

        // Page indicator
        if (_pageLabel != null)
        {
            _pageLabel.Text = $"{_currentPage}/{_currentNote.TotalPages - 1}";
        }

        // Arrow indicators
        if (_leftArrow != null)
        {
            _leftArrow.Visible = _currentPage > 0;
        }
        if (_rightArrow != null)
        {
            _rightArrow.Visible = _currentPage < _currentNote.TotalPages - 1;
        }

        var showingTranscript = _currentPage > 0;

        // Prepare transcript text before transition
        if (_transcriptLabel != null && showingTranscript && _currentNote.TranscriptPages != null)
        {
            int transcriptIndex = _currentPage - 1;
            if (transcriptIndex < _currentNote.TranscriptPages.Length)
            {
                _transcriptLabel.Text = _currentNote.TranscriptPages[transcriptIndex];
            }
        }

        if (instant)
        {
            // Instant update (used on initial open)
            if (_blurOverlay != null)
            {
                _blurOverlay.Visible = showingTranscript;
                _blurOverlay.Modulate = new Color(1, 1, 1, showingTranscript ? 1 : 0);
            }
            if (_transcriptLabel != null)
            {
                _transcriptLabel.Visible = showingTranscript;
                _transcriptLabel.Modulate = new Color(1, 1, 1, showingTranscript ? 1 : 0);
            }
        }
        else
        {
            // Animated transition
            TransitionPage(showingTranscript);
        }
    }

    private void TransitionPage(bool showTranscript)
    {
        _pageTween?.Kill();
        _isPageTransitioning = true;
        _pageTween = CreateTween();
        _pageTween.SetParallel(true);

        float targetAlpha = showTranscript ? 1.0f : 0.0f;
        var easeType = Tween.EaseType.Out;

        if (_blurOverlay != null)
        {
            if (showTranscript) _blurOverlay.Visible = true;
            _pageTween.TweenProperty(_blurOverlay, "modulate:a", targetAlpha, PageTransitionDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(easeType);
        }

        if (_transcriptLabel != null)
        {
            if (showTranscript)
            {
                _transcriptLabel.Visible = true;
                // Only reset for typewriter on new pages
                bool isNewPage = _currentPage > _highestPageVisited;
                if (_activeUseTypewriter && isNewPage)
                {
                    _transcriptLabel.VisibleRatio = 0f;
                }
                else
                {
                    _transcriptLabel.VisibleRatio = 1f;
                }
            }
            _pageTween.TweenProperty(_transcriptLabel, "modulate:a", targetAlpha, PageTransitionDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(easeType);
        }

        _pageTween.SetParallel(false);

        if (showTranscript)
        {
            // Start typewriter after fade-in completes (only for new pages)
            _pageTween.TweenCallback(Callable.From(() =>
            {
                _isPageTransitioning = false;
                bool isNewPage = _currentPage > _highestPageVisited;
                _highestPageVisited = Mathf.Max(_highestPageVisited, _currentPage);

                if (_activeUseTypewriter && isNewPage)
                {
                    StartTypewriter();
                }
                else if (_transcriptLabel != null)
                {
                    // Typewriter disabled or already seen - show full text
                    _transcriptLabel.VisibleRatio = 1f;
                }
            }));
        }
        else
        {
            // Hide elements after fade out completes
            _pageTween.TweenCallback(Callable.From(() =>
            {
                _isPageTransitioning = false;
                _typewriter.Stop();
                if (_blurOverlay != null) _blurOverlay.Visible = false;
                if (_transcriptLabel != null) _transcriptLabel.Visible = false;
            }));
        }
    }

    private void StartTypewriter()
    {
        _typewriter.Start(_transcriptLabel, TypewriterDuration);
    }
}
