using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotFeatureLibrary.GameInput;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.DebugConsole;

public partial class DebugConsoleService : Node
{
    public static DebugConsoleService Instance { get; private set; }

    public delegate string CommandHandler(string[] args);

    private readonly record struct CommandEntry(string Description, CommandHandler Handler);

    private readonly Dictionary<string, CommandEntry> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private string _savedInput = "";

    private CanvasLayer _canvasLayer;
    private PanelContainer _panel;
    private RichTextLabel _output;
    private LineEdit _input;
    private Tween _slideTween;
    private float _panelHeight;
    private bool _isOpen;

    private const float SlideDuration = 0.2f;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
        _panel = GetNode<PanelContainer>("CanvasLayer/Panel");
        _output = GetNode<RichTextLabel>("CanvasLayer/Panel/VBox/Output");
        _input = GetNode<LineEdit>("CanvasLayer/Panel/VBox/Input");

        _panelHeight = _panel.Size.Y;
        _panel.Position = new Vector2(0, -_panelHeight);

        _input.GuiInput += OnInputGuiInput;

        RegisterBuiltinCommands();
        Print("Debug Console. Type 'help' for commands.");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed(InputMapping.TOGGLE_DEBUG_CONSOLE))
        {
            Toggle();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_isOpen)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    public void RegisterCommand(string name, string description, CommandHandler handler)
    {
        _commands[name.ToLowerInvariant()] = new CommandEntry(description, handler);
    }

    public void UnregisterCommand(string name)
    {
        _commands.Remove(name.ToLowerInvariant());
    }

    private void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    private void Open()
    {
        _isOpen = true;
        _canvasLayer.Visible = true;

        _slideTween?.Kill();
        _slideTween = CreateTween();
        _slideTween.TweenProperty(_panel, "position:y", 0f, SlideDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _slideTween.TweenCallback(Callable.From(() => _input.GrabFocus()));

        EventBus.Instance?.Publish(new ConsoleOpenedEvent());
    }

    private void Close()
    {
        _isOpen = false;

        _slideTween?.Kill();
        _slideTween = CreateTween();
        _slideTween.TweenProperty(_panel, "position:y", -_panelHeight, SlideDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _slideTween.TweenCallback(Callable.From(() =>
        {
            _canvasLayer.Visible = false;
            _input.ReleaseFocus();
        }));

        EventBus.Instance?.Publish(new ConsoleClosedEvent());
    }

    private void ExecuteCommand(string rawInput)
    {
        var trimmed = rawInput.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;

        if (_history.Count == 0 || _history[^1] != trimmed)
            _history.Add(trimmed);
        _historyIndex = -1;

        PrintColored($"> {trimmed}", new Color(0.6f, 0.8f, 1f));

        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts[0].ToLowerInvariant();
        var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(commandName, out var entry))
        {
            var result = entry.Handler(args);
            if (!string.IsNullOrEmpty(result))
                Print(result);
        }
        else
        {
            PrintColored($"Unknown command: {commandName}. Type 'help' for available commands.", new Color(1f, 0.4f, 0.4f));
        }
    }

    private void Print(string text)
    {
        _output.AppendText(text + "\n");
    }

    private void PrintColored(string text, Color color)
    {
        var hex = color.ToHtml(false);
        _output.AppendText($"[color=#{hex}]{text}[/color]\n");
    }

    private void RegisterBuiltinCommands()
    {
        RegisterCommand("help", "List all commands, or 'help <command>' for details", args =>
        {
            if (args.Length > 0)
            {
                var name = args[0].ToLowerInvariant();
                if (_commands.TryGetValue(name, out var cmd))
                    return $"{name} — {cmd.Description}";
                return $"Unknown command: {name}";
            }

            var lines = _commands
                .OrderBy(kv => kv.Key)
                .Select(kv => $"  {kv.Key} — {kv.Value.Description}");
            return string.Join("\n", lines);
        });

        RegisterCommand("clear", "Clear the console output", _ =>
        {
            _output.Clear();
            return null;
        });
    }

    // -- History --

    private void OnInputGuiInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } key) return;

        if (key.Keycode == Key.Enter || key.Keycode == Key.KpEnter)
        {
            var text = _input.Text;
            _input.Clear();
            ExecuteCommand(text);
            _input.AcceptEvent();
        }
        else if (key.Keycode == Key.Up)
        {
            HistoryPrevious();
            _input.AcceptEvent();
        }
        else if (key.Keycode == Key.Down)
        {
            HistoryNext();
            _input.AcceptEvent();
        }
    }

    private void HistoryPrevious()
    {
        if (_history.Count == 0) return;

        if (_historyIndex == -1)
        {
            _savedInput = _input.Text;
            _historyIndex = _history.Count - 1;
        }
        else if (_historyIndex > 0)
        {
            _historyIndex--;
        }

        _input.Text = _history[_historyIndex];
        _input.CaretColumn = _input.Text.Length;
    }

    private void HistoryNext()
    {
        if (_historyIndex == -1) return;

        _historyIndex++;
        if (_historyIndex >= _history.Count)
        {
            _historyIndex = -1;
            _input.Text = _savedInput;
        }
        else
        {
            _input.Text = _history[_historyIndex];
        }
        _input.CaretColumn = _input.Text.Length;
    }
}
