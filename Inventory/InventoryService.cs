using System.Collections.Generic;
using Godot;
using GodotFeatureLibrary.Events;

namespace GodotFeatureLibrary.Inventory;

public partial class InventoryService : Node
{
    public static InventoryService Instance { get; private set; }

    [Export] public string ItemDirectory { get; set; } = "res://Data/Items/";

    private readonly Dictionary<string, ItemDeclaration> _declarations = new();
    private readonly Dictionary<string, ContainerData> _containers = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        LoadDeclarations(ItemDirectory);
        RegisterConsoleCommands();
    }

    private void LoadDeclarations(string directoryPath)
    {
        if (!DirAccess.DirExistsAbsolute(directoryPath))
        {
            GD.Print($"[Inventory] Item directory not found at {directoryPath}, skipping auto-load");
            return;
        }

        using var dir = DirAccess.Open(directoryPath);
        if (dir == null) return;

        dir.ListDirBegin();
        var fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && (fileName.EndsWith(".tres") || fileName.EndsWith(".res")))
            {
                var resource = GD.Load<Resource>(directoryPath.PathJoin(fileName));
                if (resource is ItemDeclaration declaration && !string.IsNullOrEmpty(declaration.Id))
                    RegisterDeclaration(declaration);
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        GD.Print($"[Inventory] Loaded {_declarations.Count} item declaration(s)");
    }

    public void RegisterDeclaration(ItemDeclaration declaration)
    {
        if (string.IsNullOrEmpty(declaration.Id))
        {
            GD.PushWarning("[Inventory] Cannot register declaration with empty Id");
            return;
        }
        _declarations[declaration.Id] = declaration;
    }

    public ItemDeclaration GetDeclaration(string declarationId)
        => _declarations.GetValueOrDefault(declarationId);

    public IEnumerable<string> GetDeclarationIds() => _declarations.Keys;

    public ContainerData CreateContainer(string containerId, int slotCount, ContainerMode mode = ContainerMode.Both)
    {
        if (_containers.ContainsKey(containerId))
        {
            GD.PushWarning($"[Inventory] Container '{containerId}' already exists, returning existing");
            return _containers[containerId];
        }

        var container = new ContainerData(containerId, slotCount, mode);
        _containers[containerId] = container;
        EventBus.Instance?.Publish(new ContainerChangedEvent(containerId, true));
        return container;
    }

    public ContainerData GetContainer(string containerId)
        => _containers.GetValueOrDefault(containerId);

    public void RemoveContainer(string containerId)
    {
        if (_containers.Remove(containerId))
            EventBus.Instance?.Publish(new ContainerChangedEvent(containerId, false));
    }

    public IEnumerable<string> GetContainerIds() => _containers.Keys;
}
