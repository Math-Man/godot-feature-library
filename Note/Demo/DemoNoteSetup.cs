using Godot;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Note.Demo;

public partial class DemoNoteSetup : Node
{
    [Export] public PackedScene NoteLayoutScene { get; set; }

    public override void _Ready()
    {
        // Configure NoteService
        var service = GetNode<NoteService>("../NoteService");
        service.NoteLayoutScene = NoteLayoutScene;
        service.UseTypewriter = true;
        service.TypewriterDuration = 3f;

        // Create note data
        var data = new NoteData
        {
            Id = "demo_note",
            Title = "A Mysterious Note",
            Image = GD.Load<Texture2D>("res://Note/Assets/Note.png"),
            TranscriptPages = new[]
            {
                "You found a note left behind by a mysterious traveler.\n\nThe paper is aged and the ink is fading, but the words are still legible.",
                "\"The key to understanding lies not in the destination, but in the journey itself.\n\nRemember: every great discovery began with a single step.\"\n\n— An Unknown Wanderer"
            }
        };

        GetNode<Note>("../Note").Data = data;

        // Release mouse while note is open so DemoMouseLook pauses
        EventBus.Instance?.Subscribe<NoteOpenedEvent>(_ => Input.MouseMode = Input.MouseModeEnum.Visible);
        EventBus.Instance?.Subscribe<NoteClosedEvent>(_ => Input.MouseMode = Input.MouseModeEnum.Captured);
    }
}
