# DialogueEngine

A typewriter-style dialogue system for Godot.

## Dependencies:
- Requires Event-Bus folder to be present

## Setup

Add `DialogueService` as a node and assign the exports:
- `DialogueLabel` - RichTextLabel for dialogue text
- `DialogueTitle` - RichTextLabel for speaker name (optional)
- `DialogueContainer` - Control to show/hide
- `TypewriterPlayer` - AudioStreamPlayer for typing sounds (optional)
- `TypewriterSounds` - Array of sounds to randomly play

## Usage

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

## Dialogue Modes

| Mode        | Interruptible  | Dismiss |
|-------------|----------------|---------|
| `Narration` | No             | Auto    |
| `Dialogue`  | Yes (5x speed) | Manual  |
| `Cutscene`  | No             | Manual  |

## Input

- **Keyboard/Gamepad**: Handled in `_UnhandledInput`
- **Mouse**: Handled in `_Input`

During interruptible dialogue, input speeds up the typewriter. After completion, input dismisses the dialogue (except Narration which auto-dismisses).