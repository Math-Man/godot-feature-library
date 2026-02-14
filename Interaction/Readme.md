# Interaction

Raycast-based interaction system for 3D games with two interaction patterns.

## Dependencies
- **EventBus**
- **GameInput** (InputMapping)
- **DialogueEngine** (DialogueData — used by Inspectable)

## Components

### IInteractable
Interface for any object that can be interacted with. Implement `OnInteract()` on your nodes.

### InteractionAllowedChangedEvent
Publish via EventBus to globally enable/disable interactions:
```csharp
EventBus.Instance.Publish(new InteractionAllowedChangedEvent(false)); // disable
EventBus.Instance.Publish(new InteractionAllowedChangedEvent(true));  // enable
```

### CursorInteraction
Free-cursor mouse interaction. Raycasts from the camera at the mouse position and calls `OnInteract()` on the first `IInteractable` found.

- **Collision Layer**: 2 (areas only)
- **Ray Length**: 50 units
- **Input**: `interact_primary`
- **Setup**: Add as an autoload or scene-level node

Traverses the node hierarchy (parent + children) to find `IInteractable` implementations.

### Inspectable
An `Area3D` that triggers dialogue when interacted with. Assign `DialogueData` resources in the editor.

**Modes:**
| Mode | Input | Behavior |
|------|-------|----------|
| `World` | `interact_primary` | FPS-style: raycast from screen center (requires captured mouse) |
| `Puzzle` | `interact_puzzle` | Mouse click when cursor is visible |

**Exports:**
| Property | Default | Description |
|----------|---------|-------------|
| `Mode` | `World` | Interaction mode |
| `MaxDistance` | `3.0` | Raycast distance |
| `Dialogues` | `[]` | Array of `DialogueData` resources, cycled on each interaction |

## Collision Layers
- **Layer 2**: Used by `CursorInteraction` for free-cursor raycasts
- **Layer 3** (bit 4): Used by `Inspectable` for self-targeted raycasts
