# Inventory

A slot-based inventory system with smart stacking, serialization, and UI.

## Dependencies
- **EventBus**
- **GameInput** (InputMapping — for `toggle_inventory` action)
- **Interaction** (IInteractable — used by ItemPickup)
- **DebugConsole** (optional — console commands)

## Setup

1. Create `ItemDeclaration` resources (`.tres`) in a directory (default: `res://Data/Items/`).
2. Add `InventoryService` as an autoload. Set the `ItemDirectory` export if your items are elsewhere.
3. Create containers at runtime:
   ```csharp
   InventoryService.Instance.CreateContainer("player", slotCount: 20);
   ```
4. Optionally instance `inventory_panel.tscn` for UI. Set the `ContainerId` export to match your container.

## Configurable Exports

| Component | Export | Default | Description |
|-----------|--------|---------|-------------|
| `InventoryService` | `ItemDirectory` | `res://Data/Items/` | Path to auto-load ItemDeclaration resources |
| `InventoryPanel` | `ContainerId` | `"player"` | Which container the panel displays |
| `ItemPickup` | `TargetContainer` | `"player"` | Which container items are added to on pickup |

## Core API

```csharp
var svc = InventoryService.Instance;

// Add/remove
svc.AddItem("player", "health_potion", 3);
svc.RemoveItem("player", "health_potion", 1);
svc.TransferItem("player", "chest", "health_potion", 2);

// Query
svc.HasItem("player", "key");
svc.GetCount("player", "health_potion");
svc.HasItemAnywhere("key");

// Save/load
var data = svc.ExportData();
svc.ImportData(data);
```

## ItemDeclaration (Resource)

Create in the Godot editor as a `.tres` file:

| Property | Default | Description |
|----------|---------|-------------|
| `Id` | `""` | Unique identifier |
| `DisplayName` | `""` | Human-readable name |
| `Descriptions` | `{}` | Key-value description strings |
| `Icon` | `null` | UI texture |
| `MaxStackSize` | `1` | Max items per slot |
| `MaxPerContainer` | `-1` | Max per container (-1 = unlimited) |
| `WorldScene` | `null` | PackedScene for 3D/2D representation |
| `Droppable` | `true` | Can be dropped |
| `Tags` | `[]` | Category tags |

## Stacking Behavior

- Items without metadata fill existing stacks first, then empty slots
- Items with metadata always get their own slot (never merged)
- Removal is LIFO (last-filled slots first)

## Events

| Event | When |
|-------|------|
| `ItemAddedEvent` | Items added to a container |
| `ItemRemovedEvent` | Items removed from a container |
| `ContainerChangedEvent` | Container created or destroyed |
| `InventoryOpenedEvent` | UI panel opened |
| `InventoryClosedEvent` | UI panel closed |

## Console Commands

Requires **DebugConsole** feature. All commands prefixed with `inventory`:

```
inventory create <id> <slots> [insert|extract|both]
inventory add <container> <item> [quantity]
inventory remove <container> <item> [quantity]
inventory list <container>
inventory has <container> <item>
inventory declarations
inventory containers
```
