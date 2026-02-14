# Godot Feature Library

A collection of reusable, modular features for Godot 4 (C# / .NET 8). Each feature is self-contained in its own folder and can be copied into any project.

## Features

| Feature | Description | Demo |
|---------|-------------|------|
| **EventBus** | Generic publish/subscribe event system. Singleton autoload. | — |
| **Typewriter** | Character-by-character text reveal with audio and custom timing curves. | — |
| **GameInput** | Centralized input action name constants and player control events. | — |
| **PostProcess** | Runtime post-processing manager with priority stacking and smooth transitions. | Yes |
| **DialogueEngine** | Typewriter dialogue system with queuing, speaker titles, and three interaction modes. | Yes |
| **DebugConsole** | Drop-down debug console with command registration, history, and auto-complete. | Yes |
| **Interaction** | Raycast-based cursor interaction system with `IInteractable` interface. | Yes |
| **Puzzle** | Puzzle mode with camera switching, LOD detail swapping, and fade transitions. | Yes |
| **Inventory** | Slot-based inventory with containers, stacking, transfers, and serialization. | Yes |
| **WorldSpaceUI** | Interactive UI screens rendered on 3D surfaces via SubViewport input injection. | Yes |
| **Overlay** | Temporary screen overlays and 2D lines anchored to 3D world positions. | Yes |
| **Note** | Collectible notes with image pages, transcript text, and typewriter animation. | Yes |

## Dependency Graph

```
No dependencies          Depends on EventBus        Depends on multiple
─────────────────        ──────────────────────      ──────────────────────────────────

EventBus                 DialogueEngine              Interaction
Typewriter                 ├─ EventBus                 ├─ EventBus
GameInput                  ├─ Typewriter               ├─ GameInput
PostProcess                └─ GameInput                └─ DialogueEngine

                         DebugConsole                Puzzle
                           ├─ EventBus                 ├─ EventBus
                           └─ GameInput                ├─ GameInput
                                                       └─ Interaction
                         Overlay
                           ├─ EventBus               Inventory
                           └─ DebugConsole*             ├─ EventBus
                                                       ├─ GameInput
                         WorldSpaceUI                   ├─ Interaction
                           ├─ EventBus*                 └─ DebugConsole*
                           └─ DialogueEngine*
                                                     Note
                                                       ├─ EventBus
                                                       ├─ GameInput
                                                       ├─ Interaction
                                                       └─ Typewriter

                         * = optional dependency
```

## Quick Start

1. Copy the feature folder(s) you need into your project
2. Register autoloads (see below)
3. Define the input actions referenced by `GameInput/InputMapping.cs` in your project's Input Map
4. See each feature's `Readme.md` for detailed setup instructions

## Autoloads

Features that use the singleton pattern need to be registered as script autoloads or added to your scene tree:

| Feature | Required | Path |
|---------|----------|------|
| **EventBus** | By most features | `EventBus/EventBus.cs` |
| **OverlayService** | By Overlay | `Overlay/OverlayService.cs` |

Other singletons (`NoteService`, `PostProcessManager`, `InventoryService`, etc.) can be added as scene nodes or autoloads depending on your project's needs.

## Input Actions

Features reference input actions through `GameInput/InputMapping.cs`. Define these in your project's Input Map as needed:

| Constant | Action Name | Used By |
|----------|-------------|---------|
| `INTERACTION_PRIMARY` | `interact_primary` | DialogueEngine, Interaction, Note |
| `INTERACTION_SECONDARY` | `interact_secondary` | Puzzle, Note |
| `INTERACTION_PUZZLE` | `interact_puzzle` | Interaction, Note |
| `TOGGLE_DEBUG_CONSOLE` | `toggle_debug_console` | DebugConsole |
| `TOGGLE_INVENTORY` | `toggle_inventory` | Inventory |

## Demos

Most features include a `Demo/` subfolder with a runnable scene and helper scripts. These demonstrate how to set up and use the feature:

| Feature | Demo Scene |
|---------|------------|
| DialogueEngine | `DialogueEngine/dialogue_demo.tscn` |
| DebugConsole | `DebugConsole/demo/debug_console_demo.tscn` |
| Interaction | `Interaction/Demo/interaction_demo.tscn` |
| Inventory | `Inventory/Demo/inventory_demo.tscn` |
| WorldSpaceUI | `WorldSpaceUI/Demo/worldspaceui_demo.tscn` |
| PostProcess | `PostProcess/Demo/postprocess_demo.tscn` |
| Overlay | `Overlay/Demo/overlay_demo.tscn` |
| Note | `Note/Demo/note_demo.tscn` |
| Puzzle | `Puzzle/Demo/puzzle_demo.tscn` |

## Project Structure

```
GodotFeatureLibrary/
├── EventBus/            # Pub/sub event system (autoload)
├── Typewriter/          # Text reveal effect
├── GameInput/           # Input constants & events
├── PostProcess/         # Shader effect manager + demo
├── DialogueEngine/      # Dialogue system + demo
├── DebugConsole/        # Drop-down console + demo
├── Interaction/         # Cursor raycasting + IInteractable + demo
├── Puzzle/              # Puzzle mode controller + demo
├── Inventory/           # Item system + UI + demo
├── WorldSpaceUI/        # 3D screen interaction + demo
├── Overlay/             # Screen overlay system + demo
└── Note/                # Collectible notes + assets + demo
```

## Requirements

- Godot 4.6+
- .NET 8.0

## License

Audio assets include attribution where required — see `Attribution.txt` files in asset folders.
