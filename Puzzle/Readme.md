# Puzzle

A puzzle mode system with camera switching, LOD detail swapping, and fade transitions.

## Dependencies
- **EventBus**
- **GameInput** (InputMapping — for `interact_secondary` to exit)
- **Interaction** (IInteractable)

## Setup

1. Attach `PuzzleInteraction` as a child of an `Area3D` (the interaction trigger).
2. Assign a `PuzzleCamera` in the inspector.
3. Optionally set `GeometryRootName` and detail suffixes for LOD swapping.

## Usage

The player interacts with the Area3D → `OnInteract()` is called → puzzle mode enters with a fade-to-black transition, camera switch, and LOD swap. Press `interact_secondary` (right-click) to exit.

## Exports

| Property | Default | Description |
|----------|---------|-------------|
| `PuzzleCamera` | — | Camera to switch to in puzzle mode |
| `GeometryRootName` | — | Name of node containing LOD geometry |
| `LowDetailSuffix` | `_low` | Suffix for low-detail nodes |
| `HighDetailSuffix` | `_high` | Suffix for high-detail nodes |
| `FadeDuration` | `0.3` | Fade transition duration in seconds |
| `FadeColor` | `Black` | Fade overlay color |

## Events

| Event | When |
|-------|------|
| `PuzzleEnteredEvent` | After entering puzzle mode (post-fade) |
| `PuzzleExitedEvent` | After exiting puzzle mode (pre-fade-in) |

## Detail Swap

If `GeometryRootName` is set, the system searches for child nodes ending with `_low` and `_high` suffixes. In puzzle mode, low-detail nodes are hidden and high-detail nodes are shown.
