# Overlay

Displays temporary visual overlays on screen — fixed position, world-anchored, or bounds-tracked. Also draws 2D lines projected from 3D world positions.

## Dependencies
- **EventBus** (for `OverlayPauseEvent`)
- **DebugConsole** (test trigger only)

## Setup

Register `OverlayService` as a script autoload in Project Settings.

## Overlay Types

| Method | Description |
|--------|-------------|
| `Show()` | Fixed screen position, timed |
| `ShowAnchored()` | Tracks a Node3D, projected to screen each frame |
| `ShowBounds()` | Resizes to fit projected AABB of a Node3D |

## Line Types

| Method | Description |
|--------|-------------|
| `ShowLine(V3, V3)` | Line between two world positions |
| `ShowLine(V2, V3)` | Line from screen point to world position |
| `ShowLines(V2[], V3[])` | Multiple screen-to-world lines (optional auto-pairing) |
| `ShowLines(V3[], V3[])` | Multiple world-to-world lines |
| `ShowBoundsConnector()` | Lines from screen rect corners to target AABB |

## Common Parameters

| Parameter | Description |
|-----------|-------------|
| `duration` | Lifetime in seconds. `<= 0` means infinite (cancel manually) |
| `fadeIn` | Fade-in duration in seconds |
| `fadeOut` | Fade-out duration in seconds |
| `maxDistance` | Hide when camera is farther than this. `<= 0` means no limit |

## Management

```csharp
var id = OverlayService.Instance.Show(scene, pos, 2f);
OverlayService.Instance.Cancel(id);     // cancel specific
OverlayService.Instance.CancelAll();    // cancel all
```

## Pausing

Publish `OverlayPauseEvent(true)` to hide all overlays, `OverlayPauseEvent(false)` to restore.

## Components

| File | Purpose |
|------|---------|
| `OverlayService.cs` | Singleton service, lifecycle management |
| `OverlayService.Overlays.cs` | Show/ShowAnchored/ShowBounds + overlay processing |
| `OverlayService.Lines.cs` | ShowLine/ShowLines + line processing |
| `OverlayService.LineGroups.cs` | Auto-paired line groups with optimal assignment |
| `OverlayGeometry.cs` | Static AABB utilities for bounds projection |
| `OverlayPauseEvent.cs` | Event to show/hide all overlays |
| `OverlayTestTrigger.cs` | Test trigger with console commands and key shortcuts |
