using Godot;
using GodotFeatureLibrary.GameInput;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Inventory.UI;

public partial class InventoryPanel : Node
{
    [Export] public string ContainerId { get; set; } = "player";

    private const float SlideDuration = 0.2f;
    private const float IconSize = 40f;

    private PanelContainer _panel;
    private VBoxContainer _itemList;
    private Label _descriptionLabel;
    private int _selectedSlotIndex = -1;
    private bool _isOpen;

    public override void _Ready()
    {
        _panel = GetNode<PanelContainer>("CanvasLayer/Panel");
        _itemList = GetNode<VBoxContainer>("CanvasLayer/Panel/Content/ItemList");

        _descriptionLabel = GetNode<Label>("CanvasLayer/Panel/Content/Description");
        _descriptionLabel.Text = "";

        _panel.Visible = false;

        EventBus.Instance?.Subscribe<ItemAddedEvent>(OnInventoryChanged);
        EventBus.Instance?.Subscribe<ItemRemovedEvent>(OnInventoryChanged);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed(InputMapping.TOGGLE_INVENTORY))
        {
            Toggle();
            GetViewport().SetInputAsHandled();
        }
    }

    // Consume unhandled input while open — blocks other systems but lets GUI clicks through
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_isOpen)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void Toggle()
    {
        if (_isOpen)
            Close();
        else
            Open();
    }

    private void Open()
    {
        _isOpen = true;
        _selectedSlotIndex = -1;
        if (_descriptionLabel != null) _descriptionLabel.Text = "";

        Input.MouseMode = Input.MouseModeEnum.Visible;

        RefreshDisplay();

        _panel.Visible = true;
        _panel.Modulate = _panel.Modulate with { A = 0f };

        var tween = CreateTween();
        tween.TweenProperty(_panel, "modulate:a", 1f, SlideDuration);

        EventBus.Instance?.Publish(new InventoryOpenedEvent());
    }

    private void Close()
    {
        _isOpen = false;
        _selectedSlotIndex = -1;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        var tween = CreateTween();
        tween.TweenProperty(_panel, "modulate:a", 0f, SlideDuration);
        tween.TweenCallback(Callable.From(() => _panel.Visible = false));

        EventBus.Instance?.Publish(new InventoryClosedEvent());
    }

    private void RefreshDisplay()
    {
        // Clear existing rows
        foreach (var child in _itemList.GetChildren())
        {
            child.QueueFree();
        }

        var container = InventoryService.Instance?.GetContainer(ContainerId);
        if (container == null) return;

        foreach (var (index, stack) in container.GetOccupiedSlots())
        {
            var declaration = InventoryService.Instance.GetDeclaration(stack.DeclarationId);
            if (declaration == null) continue;

            var row = CreateItemRow(index, stack, declaration);
            _itemList.AddChild(row);
        }
    }

    private Control CreateItemRow(int slotIndex, ItemStack stack, ItemDeclaration declaration)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 6);

        // Icon
        var icon = new TextureRect
        {
            Texture = declaration.Icon,
            CustomMinimumSize = new Vector2(IconSize, IconSize),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        row.AddChild(icon);

        // Name
        var nameLabel = new Label
        {
            Text = declaration.DisplayName,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        nameLabel.AddThemeColorOverride("font_color", Colors.White);
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        row.AddChild(nameLabel);

        // Quantity (only if > 1)
        if (stack.Quantity > 1)
        {
            var qtyLabel = new Label
            {
                Text = $"x{stack.Quantity}",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            qtyLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            qtyLabel.AddThemeFontSizeOverride("font_size", 11);
            row.AddChild(qtyLabel);
        }

        // Click handling
        row.MouseFilter = Control.MouseFilterEnum.Stop;
        row.GuiInput += (e) => OnRowInput(e, slotIndex, row);

        return row;
    }

    private void OnRowInput(InputEvent @event, int slotIndex, HBoxContainer row)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;

        _selectedSlotIndex = slotIndex;
        HighlightRow(row);

        var container = InventoryService.Instance?.GetContainer(ContainerId);
        var stack = container?.GetSlot(slotIndex);
        if (stack == null) return;

        var declaration = InventoryService.Instance.GetDeclaration(stack.DeclarationId);
        if (declaration == null) return;

        var description = "No description";
        if (declaration.Descriptions.Count > 0)
        {
            foreach (var entry in declaration.Descriptions)
            {
                description = entry.Value;
                break;
            }
        }

        if (_descriptionLabel != null)
            _descriptionLabel.Text = description;
    }

    private void HighlightRow(HBoxContainer selectedRow)
    {
        // Reset all rows
        foreach (var child in _itemList.GetChildren())
        {
            if (child is HBoxContainer hbox)
                hbox.Modulate = Colors.White;
        }

        // Highlight selected
        selectedRow.Modulate = new Color(1.2f, 1.2f, 0.8f);
    }

    private void OnInventoryChanged(ItemAddedEvent e)
    {
        if (_isOpen && e.ContainerId == ContainerId)
            RefreshDisplay();
    }

    private void OnInventoryChanged(ItemRemovedEvent e)
    {
        if (_isOpen && e.ContainerId == ContainerId)
            RefreshDisplay();
    }
}
