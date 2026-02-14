using Godot;

namespace GodotFeatureLibrary.Note;

[GlobalClass]
public partial class NoteData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string Title { get; set; } = "";
    [Export] public Texture2D Image { get; set; }
    [Export(PropertyHint.MultilineText)] public string[] TranscriptPages { get; set; } = [];

    /// <summary>
    /// Total pages: 1 (image page) + transcript pages
    /// </summary>
    public int TotalPages => 1 + (TranscriptPages?.Length ?? 0);
}
