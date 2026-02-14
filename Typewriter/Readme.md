# Typewriter

A reusable typewriter effect for RichTextLabels.

## Dependencies
- None

## Setup

Create a `TypewriterEffect` node as a child (or instantiate it from code) and call `Update()` from your `_Process()`.

## Usage

```csharp
// In _Ready():
_typewriter = new TypewriterEffect
{
    Sounds = myAudioStreams,
    PitchVariation = 0.05f,
    SoundCooldown = 0.08f,
    FastForwardMultiplier = 4f
};
AddChild(_typewriter);

// Start the effect on a label:
_typewriter.Start(myRichTextLabel, duration: 2f, curve: myOptionalCurve);

// In _Process():
_typewriter.Update((float)delta);

// Speed up:
_typewriter.SetFastForward(true);

// Check state:
if (_typewriter.IsComplete) { /* ... */ }
```

## Properties

| Export              | Default | Description                              |
|---------------------|---------|------------------------------------------|
| `Sounds`            | `[]`    | Audio streams to randomly play per character |
| `PitchVariation`    | `0.05`  | Random pitch offset (±)                  |
| `SoundCooldown`     | `0.08`  | Minimum seconds between sounds           |
| `FastForwardMultiplier` | `4.0` | Speed multiplier during fast-forward     |

## API

| Method / Property   | Description                                     |
|---------------------|-------------------------------------------------|
| `Start(label, duration, curve?)` | Begin typewriter on a RichTextLabel |
| `Update(delta)`     | Advance the effect (call from `_Process`)        |
| `SetFastForward(bool)` | Toggle fast-forward mode                     |
| `Complete()`        | Instantly finish revealing all text              |
| `Stop()`            | Cancel without completing                        |
| `IsActive`          | True while animating                             |
| `IsComplete`        | True after all text is revealed                  |