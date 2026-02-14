# DebugConsole

A slide-down in-game debug console with a command registry and history.

## Dependencies
- **EventBus**
- **GameInput** (InputMapping — for `toggle_debug_console` action)

## Setup

1. Add `debug_console.tscn` as an autoload or instance it in your scene.
2. Map the `toggle_debug_console` input action in Project Settings.

## Usage

### Registering commands

```csharp
DebugConsoleService.Instance.RegisterCommand(
    "teleport",
    "Teleport player to x y z",
    args =>
    {
        // parse args, do work...
        return "Teleported!";  // return null for no output
    }
);
```

### Unregistering commands

```csharp
DebugConsoleService.Instance.UnregisterCommand("teleport");
```

### Reacting to console open/close

```csharp
EventBus.Instance.Subscribe<ConsoleOpenedEvent>(_ => PauseGame());
EventBus.Instance.Subscribe<ConsoleClosedEvent>(_ => ResumeGame());
```

## Built-in Commands

| Command | Description |
|---------|-------------|
| `help` | List all registered commands |
| `help <name>` | Show description for a specific command |
| `clear` | Clear console output |

## Features

- **Slide animation**: 0.2s cubic ease-out
- **Command history**: Up/Down arrows to navigate previous commands
- **Case-insensitive**: Commands are matched regardless of case
- **Input capture**: While open, all unhandled input is consumed (prevents game input leaking through)
- **Canvas layer 200**: Renders above all other UI

## Events

| Event | When |
|-------|------|
| `ConsoleOpenedEvent` | Console slides open |
| `ConsoleClosedEvent` | Console slides closed |
