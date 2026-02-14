# GameInput

Centralized input action name constants and player control state events.

## Dependencies

- **EventBus** (for PlayerControlsEvent)

## Files

| File | Purpose |
|------|---------|
| `InputMapping.cs` | String constants for all input action names |
| `PlayerControlsEvent.cs` | Event to enable/disable player controls via EventBus |

## InputMapping

Avoids hardcoded action name strings scattered throughout the project. Reference these constants instead:

```csharp
if (Input.IsActionJustPressed(InputMapping.INTERACTION_PRIMARY)) { ... }
```

### Action Names

| Constant | Action Name | Category |
|----------|-------------|----------|
| `PLAYER_FORWARD` | `move_forward` | Movement |
| `PLAYER_BACKWARD` | `move_back` | Movement |
| `PLAYER_LEFT` | `move_left` | Movement |
| `PLAYER_RIGHT` | `move_right` | Movement |
| `PLAYER_JUMP` | `move_jump` | Movement |
| `PLAYER_SPRINT` | `move_sprint` | Movement |
| `INTERACTION_PRIMARY` | `interact_primary` | Interaction |
| `INTERACTION_SECONDARY` | `interact_secondary` | Interaction |
| `INTERACTION_PUZZLE` | `interact_puzzle` | Interaction |
| `HELD_OBJECT_PULL` | `held_object_pull` | Interaction |
| `HELD_OBJECT_PUSH` | `held_object_push` | Interaction |
| `EXIT_GAME` | `exit` | System |
| `TOGGLE_MOUSE_LOCK` | `toggle_mouse_lock` | System |
| `TOGGLE_DEBUG_UI` | `toggle_debug_ui` | UI |
| `TOGGLE_PROPERTY_INSPECTOR` | `toggle_property_inspector` | UI |
| `TOGGLE_DEBUG_CONSOLE` | `toggle_debug_console` | UI |
| `TOGGLE_INVENTORY` | `toggle_inventory` | UI |
| `NUM_KEY_1` – `NUM_KEY_5` | `num_key_1` – `num_key_5` | Hotbar |

You must define these actions in Project Settings > Input Map for them to work.

## PlayerControlsEvent

Published via EventBus to temporarily disable/enable player input (e.g. during dialogue or cutscenes):

```csharp
// Lock controls
EventBus.Instance.Publish(new PlayerControlsEvent(false));

// Unlock controls
EventBus.Instance.Publish(new PlayerControlsEvent(true));
```

Subscribe in your player controller:

```csharp
EventBus.Instance.Subscribe<PlayerControlsEvent>(e => _canMove = e.Enabled);
```
