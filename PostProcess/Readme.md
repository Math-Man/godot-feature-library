# PostProcess

A generic post-processing effect manager with priority-based stacking and smooth transitions. Bring your own shaders.

## Dependencies
- None (standalone)

## Setup

1. Create a scene with a `PostProcessManager` node.
2. Under it, add a `CanvasLayer` with a `Control` container.
3. For each shader, add a `BackBufferCopy` (set `copy_mode = Viewport`) with a `ColorRect` child (with `ShaderMaterial`).
4. Set `Mouse Filter` to `Ignore` on the container so it doesn't block input.
5. Assign the container path to `EffectsContainerPath`.
6. The manager auto-discovers shaders by `ColorRect` node name and auto-sizes them to fill the viewport.

```
PostProcessManager
└── CanvasLayer
    └── EffectsContainer (Control, mouse_filter=Ignore)
        ├── BackBufferCopy (copy_mode=Viewport)
        │   └── MyEffect (ColorRect + ShaderMaterial)
        ├── BackBufferCopy (copy_mode=Viewport)
        │   └── AnotherEffect (ColorRect + ShaderMaterial)
        └── ...
```

## Usage

### Applying effects

Use the builder pattern:

```csharp
var effect = PostProcessEffect.Create("my-glitch")
    .WithPriority(10)
    .WithTransition(0.3f)
    .Shader("MyEffect", visible: true)
        .Set("intensity", 0.8f)
        .Set("speed", 2.0f)
    .Build();

PostProcessManager.Instance.Apply(effect);
```

### Removing effects

```csharp
// Fade out over the effect's transition duration
PostProcessManager.Instance.Remove("my-glitch");

// Fade out with custom duration
PostProcessManager.Instance.Remove("my-glitch", fadeDuration: 1.0f);

// Remove instantly (no fade)
PostProcessManager.Instance.RemoveInstant("my-glitch");

// Remove all effects whose ID starts with a prefix
PostProcessManager.Instance.RemoveDomain("my-");
```

### Querying state

```csharp
PostProcessManager.Instance.HasEffect("my-glitch");  // specific effect
PostProcessManager.Instance.HasDomain("my-");         // any effect with prefix
```

## How It Works

- **Shader discovery**: On `_Ready()`, scans `EffectsContainer` for `BackBufferCopy` → `ColorRect` chains with `ShaderMaterial`. Each is registered by its node name.
- **Priority stacking**: Multiple effects can target the same shader. They're applied in priority order (ascending), each lerped by its transition progress.
- **Parameter interpolation**: Supports `float`, `int`, `Vector2/3/4`, `Color`, and `bool`. Base parameters are captured on first use and restored when all effects are removed.
- **Transitions**: Effects fade in/out by interpolating between base and target values.

## API Reference

### PostProcessEffect (Builder)

| Method | Description |
|--------|-------------|
| `Create(id)` | Start building an effect |
| `WithPriority(int)` | Set stacking priority (lower = applied first) |
| `WithTransition(float)` | Set fade-in/out duration in seconds |
| `Shader(name, visible?, params)` | Target a shader by node name |
| `Set(param, value)` | Set a parameter on the current shader |
| `Build()` | Return the finished effect |

### PostProcessManager

| Method | Description |
|--------|-------------|
| `Apply(effect)` | Apply an effect (replaces existing with same ID) |
| `Remove(id, fadeDuration?)` | Fade out an effect |
| `RemoveInstant(id)` | Remove immediately |
| `RemoveDomain(prefix, fadeDuration?)` | Remove all effects matching prefix |
| `HasEffect(id)` | Check if an effect is active |
| `HasDomain(prefix)` | Check if any effect matches prefix |
