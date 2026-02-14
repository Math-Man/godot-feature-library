# Note

A collectible note system with image pages, transcript text, typewriter effect, and audio feedback.

## Dependencies
- **EventBus** (for `NoteOpenedEvent` / `NoteClosedEvent`)
- **GameInput** (InputMapping for interaction actions)
- **Interaction** (InspectableMode enum for World/Puzzle modes)
- **Typewriter** (TypewriterEffect for transcript text animation)

## Setup

1. Register `NoteService` as a script autoload or add to your scene
2. Assign a `NoteLayoutScene` (a Control scene with the expected child nodes)
3. Configure audio and typewriter settings in the inspector

## Note Layout Scene

The layout scene must have these named child nodes:

| Node Name | Type | Purpose |
|-----------|------|---------|
| `NoteImage` | TextureRect | Displays the note image (page 0) |
| `BlurOverlay` | Control | Dark overlay shown on transcript pages |
| `TranscriptLabel` | RichTextLabel | Displays transcript text |
| `PageLabel` | Label | Shows "X/Y" page indicator |
| `LeftArrow` | Label | Previous page indicator |
| `RightArrow` | Label | Next page indicator |

## Usage

### Creating Note Data
Create `NoteData` resources in the editor (`[GlobalClass]` registered):
- **Id** — Unique ID for collection tracking
- **Title** — Display name
- **Image** — Texture for page 0
- **TranscriptPages** — Array of text pages (BBCode supported)

### Placing Notes in the World
Add a `Note` (Area3D) to your scene:
- Assign a `NoteData` resource
- Choose `Mode`: World (crosshair) or Puzzle (mouse click)
- Optionally override layout, audio, and typewriter settings per instance

### Interacting
- Primary action opens the note / advances to next page / fast-forwards typewriter
- Secondary action goes to previous page
- On last page, primary action closes the note

## Events

| Event | When |
|-------|------|
| `NoteOpenedEvent` | After opening a note |
| `NoteClosedEvent` | After closing a note |

## Collection Tracking

```csharp
NoteService.Instance.HasCollected("note_id");
NoteService.Instance.GetCollectedNoteIds();
NoteService.Instance.LoadCollectedNotes(savedIds);
```

## Assets

Audio assets included are free to use:
- `paper_scrunch.wav`, `paper_sort.wav`, `paper_move.wav` — Paper sounds
- `PaperCrackle/` — CC0 sounds by IsakIzzy ([freesound.org/s/572018](https://freesound.org/s/572018/))
- `Note.png` — Placeholder note image

## Components

| File | Purpose |
|------|---------|
| `NoteData.cs` | Resource class for note content |
| `NoteAudio.cs` | Audio manager with per-note overrides |
| `Note.cs` | Area3D interactable (World/Puzzle modes) |
| `NoteService.cs` | Singleton managing UI, pages, typewriter, collection |
