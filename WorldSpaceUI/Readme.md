# World Space UI

Interactive screens/terminals rendered in 3D space via SubViewport projection.

## Dependencies

None. The core components have no feature dependencies.

## Quick Setup

1. Instance `WorldSpaceScreen.tscn` in your scene
2. Add your UI content as children of `SubViewport/Control`
3. The current camera is detected automatically

## Sizing

On `ScreenMesh`, use the editor sync buttons:
- **Sync Viewport to Mesh** — adjust viewport resolution to match mesh (use when mesh size is fixed from Blender)
- **Sync Mesh to Viewport** — adjust mesh size to match viewport resolution

## Collision Layer

Screen uses layer 8 (value 128) for raycast detection. Don't put other colliders on this layer.

## Components

| File | Purpose |
|------|---------|
| `WorldSpaceScreen.tscn` | Pre-wired scene, instance this |
| `WorldSpaceScreenInput.cs` | Raycasts mouse to SubViewport, injects input events |
| `WorldSpaceScreenSync.cs` | Syncs mesh/viewport/collider sizes with editor buttons |
| `FakeCursor.cs` | Cursor that follows mouse inside the SubViewport UI |

## Pointer Modes

Set `Mode` on the `ScreenInput` node:

| Mode | Description |
|------|-------------|
| `FreeCursor` | Uses actual mouse position (default) |
| `Crosshair` | Uses screen center as pointer (FPS-style) |

## Cursor

Replace the cursor texture on `SubViewport/Control/Cursor`. Set `Mouse Filter` to `Ignore`.

## How It Works

1. Camera becomes current → `ScreenInput` auto-activates
2. Each frame: raycast from camera through pointer position
3. If hit screen → convert 3D hit to 2D viewport UV coordinates
4. Inject `InputEventMouseMotion`/`InputEventMouseButton` into SubViewport
5. UI inside SubViewport receives input as if mouse was there
