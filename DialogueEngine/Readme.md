# DialogueEngine

A typewriter-style dialogue system with queuing, speaker titles, and custom timing curves.

## Dependencies
- **EventBus**
- **Typewriter** (TypewriterEffect for text animation)
- **GameInput** (InputMapping for input actions)

## Setup

1. Instance `dialogue_box.tscn` in your scene (or create your own UI)
2. The scene contains a `DialogueService` node with exports pre-wired
3. Ensure `EventBus` is registered as an autoload

## Usage

### From Code
Publish a `DialogueEvent` via EventBus:

```csharp
EventBus.Instance.Publish(new DialogueEvent(
    content: "Hello, world!",
    mode: DialogueMode.Dialogue,
    duration: 2f,
    lingerDuration: 1f,
    title: "NPC",
    titleColor: Colors.Yellow
));
```

### From Editor Resources
Create `DialogueData` resources in the editor (`[GlobalClass]` registered):

```csharp
[Export] public DialogueData MyDialogue { get; set; }

// Publish:
EventBus.Instance.Publish(MyDialogue.ToEvent());
```

## Dialogue Modes

| Mode | Interruptible | Dismiss | Locks Controls |
|------|---------------|---------|----------------|
| `Narration` | No | Auto (after linger) | No |
| `Dialogue` | Yes (fast-forward) | Manual (click/key) | Yes |
| `Cutscene` | No | Manual (click/key) | Yes |

## Events

| Event | When |
|-------|------|
| `DialogueStartedEvent` | Dialogue/Cutscene mode begins (carries `Mode`) |
| `DialogueDismissedEvent` | Dialogue/Cutscene mode ends (carries `Mode`) |

Subscribe to these to lock/unlock player controls, pause systems, etc.

## DialogueEvent Properties

| Property | Default | Description |
|----------|---------|-------------|
| `Content` | — | Text to display |
| `Mode` | `Dialogue` | Narration, Dialogue, or Cutscene |
| `TypewriterCurve` | `null` | Custom timing curve (0-1 progress, 0-1 visibility) |
| `Duration` | `2f` | Typewriter animation duration in seconds |
| `LingerDuration` | `1f` | Time to wait after typewriter completes |
| `Title` | `null` | Speaker name (hidden if null/empty) |
| `TitleColor` | `null` | Speaker name color override |
| `Override` | `false` | If true, clears queue and replaces current dialogue |

## DialogueService Exports

| Property | Default | Description |
|----------|---------|-------------|
| `DialogueLabel` | — | RichTextLabel for dialogue text |
| `DialogueTitle` | — | RichTextLabel for speaker name |
| `DialogueContainer` | — | Control to show/hide |
| `TypewriterSounds` | — | Array of sounds for typewriter effect |
| `PitchVariation` | `0.05` | Random pitch variation for sounds |
| `SoundCooldown` | `0.10` | Minimum time between sounds |
| `FastForwardMultiplier` | `5` | Speed multiplier when fast-forwarding |

## Input Handling

- **Mouse**: Handled in `_Input` (catches before UI elements)
- **Keyboard/Gamepad**: Handled in `_UnhandledInput` (catches after other systems)

During interruptible dialogue, input fast-forwards the typewriter. After typewriter + linger complete, input dismisses. Narration auto-dismisses.

## Queuing

Multiple dialogue events are queued automatically. When `Override` is true, the queue is cleared and the new dialogue replaces the current one immediately.

## Components

| File | Purpose |
|------|---------|
| `DialogueService.cs` | Core service — subscribes to events, manages UI and typewriter |
| `DialogueEvent.cs` | Immutable event data published via EventBus |
| `DialogueData.cs` | `[GlobalClass]` Resource for editor authoring with `ToEvent()` |
| `DialogueMode.cs` | Enum: Narration, Dialogue, Cutscene |
| `DialogueStateEvents.cs` | `DialogueStartedEvent` / `DialogueDismissedEvent` |
| `DialogueTest.cs` | Demo script — King in Yellow excerpt |
| `dialogue_box.tscn` | Pre-wired dialogue UI scene |
| `dialogue_demo.tscn` | Demo scene with 3D environment |
| `Sound/` | Typewriter key-press sounds (CC0, see Attribution.txt) |
