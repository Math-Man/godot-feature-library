using System.Collections.Generic;
using System.Linq;
using GodotFeatureLibrary.DebugConsole;

namespace GodotFeatureLibrary.Inventory;

public partial class InventoryService
{
    private void RegisterConsoleCommands()
    {
        DebugConsoleService.Instance?.RegisterCommand("inventory",
            "Inventory management: create, add, remove, list, has, declarations, containers", args =>
            {
                if (args.Length == 0)
                    return "Usage: inventory <create|add|remove|list|has|declarations|containers> [args...]";

                return args[0].ToLowerInvariant() switch
                {
                    "create" => CmdCreate(args),
                    "add" => CmdAdd(args),
                    "remove" => CmdRemove(args),
                    "list" => CmdList(args),
                    "has" => CmdHas(args),
                    "declarations" => CmdDeclarations(),
                    "containers" => CmdContainers(),
                    _ => $"Unknown subcommand: {args[0]}"
                };
            });
    }

    private string CmdCreate(string[] args)
    {
        if (args.Length < 3) return "Usage: inventory create <id> <slots> [insert|extract|both]";
        var id = args[1];
        if (!int.TryParse(args[2], out var slots)) return "Invalid slot count";
        var mode = ContainerMode.Both;
        if (args.Length > 3)
        {
            mode = args[3].ToLowerInvariant() switch
            {
                "insert" => ContainerMode.InsertOnly,
                "extract" => ContainerMode.ExtractOnly,
                _ => ContainerMode.Both
            };
        }
        CreateContainer(id, slots, mode);
        return $"Container '{id}' created ({slots} slots, {mode})";
    }

    private string CmdAdd(string[] args)
    {
        if (args.Length < 3) return "Usage: inventory add <container> <item> [quantity]";
        var containerId = args[1];
        var itemId = args[2];
        int qty = args.Length > 3 && int.TryParse(args[3], out var parsed) ? parsed : 1;
        var result = AddItem(containerId, itemId, qty);
        return result.Success
            ? $"Added {result.QuantityAffected}x {itemId} to {containerId}" +
              (result.QuantityRemaining > 0 ? $" ({result.QuantityRemaining} didn't fit)" : "")
            : $"Failed: {result.Error}";
    }

    private string CmdRemove(string[] args)
    {
        if (args.Length < 3) return "Usage: inventory remove <container> <item> [quantity]";
        var containerId = args[1];
        var itemId = args[2];
        int qty = args.Length > 3 && int.TryParse(args[3], out var parsed) ? parsed : 1;
        var result = RemoveItem(containerId, itemId, qty);
        return result.Success
            ? $"Removed {result.QuantityAffected}x {itemId} from {containerId}"
            : $"Failed: {result.Error}";
    }

    private string CmdList(string[] args)
    {
        if (args.Length < 2) return "Usage: inventory list <container>";
        var container = GetContainer(args[1]);
        if (container == null) return $"Container '{args[1]}' not found";

        var lines = new List<string>
        {
            $"Container '{container.Id}' ({container.SlotCount} slots, {container.EmptySlotCount()} empty, {container.Mode})"
        };

        foreach (var (index, stack) in container.GetOccupiedSlots())
        {
            var decl = GetDeclaration(stack.DeclarationId);
            var name = decl?.DisplayName ?? stack.DeclarationId;
            var meta = stack.Metadata.Count > 0 ? $" [+{stack.Metadata.Count} meta]" : "";
            lines.Add($"  [{index}] {stack.Quantity}x {name}{meta}");
        }

        return string.Join("\n", lines);
    }

    private string CmdHas(string[] args)
    {
        if (args.Length < 3) return "Usage: inventory has <container> <item>";
        var count = GetCount(args[1], args[2]);
        return count > 0 ? $"{args[1]} has {count}x {args[2]}" : $"{args[1]} does not have {args[2]}";
    }

    private string CmdDeclarations()
    {
        if (_declarations.Count == 0) return "No declarations loaded";
        var lines = _declarations.Values
            .OrderBy(d => d.Id)
            .Select(d => $"  {d.Id} — {d.DisplayName} (stack:{d.MaxStackSize}, perContainer:{d.MaxPerContainer})");
        return "Declarations:\n" + string.Join("\n", lines);
    }

    private string CmdContainers()
    {
        if (_containers.Count == 0) return "No containers registered";
        var lines = _containers.Values
            .Select(c => $"  {c.Id} — {c.SlotCount} slots, {c.EmptySlotCount()} empty, {c.Mode}");
        return "Containers:\n" + string.Join("\n", lines);
    }
}
