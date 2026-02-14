using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.Inventory;

public partial class InventoryService
{
    public Godot.Collections.Dictionary ExportData()
    {
        var data = new Godot.Collections.Dictionary();

        foreach (var (id, container) in _containers)
        {
            var containerDict = new Godot.Collections.Dictionary
            {
                ["slotCount"] = container.SlotCount,
                ["mode"] = (int)container.Mode
            };

            var slotsArray = new Godot.Collections.Array();
            for (int i = 0; i < container.SlotCount; i++)
            {
                var stack = container.GetSlot(i);
                if (stack == null)
                {
                    slotsArray.Add(new Godot.Collections.Dictionary());
                }
                else
                {
                    var stackDict = new Godot.Collections.Dictionary
                    {
                        ["declarationId"] = stack.DeclarationId,
                        ["quantity"] = stack.Quantity
                    };

                    if (stack.Metadata.Count > 0)
                    {
                        var metaDict = new Godot.Collections.Dictionary();
                        foreach (var (key, value) in stack.Metadata)
                            metaDict[key] = value;
                        stackDict["metadata"] = metaDict;
                    }

                    slotsArray.Add(stackDict);
                }
            }

            containerDict["slots"] = slotsArray;
            data[id] = containerDict;
        }

        return data;
    }

    public void ImportData(Godot.Collections.Dictionary data)
    {
        _containers.Clear();

        foreach (var key in data.Keys)
        {
            var containerId = key.AsString();
            var containerDict = data[key].AsGodotDictionary();

            int slotCount = containerDict["slotCount"].AsInt32();
            var mode = (ContainerMode)containerDict["mode"].AsInt32();

            var container = new ContainerData(containerId, slotCount, mode);
            _containers[containerId] = container;

            if (!containerDict.ContainsKey("slots")) continue;

            var slotsArray = containerDict["slots"].AsGodotArray();
            for (int i = 0; i < slotsArray.Count && i < slotCount; i++)
            {
                var slotDict = slotsArray[i].AsGodotDictionary();
                if (!slotDict.ContainsKey("declarationId")) continue;

                var declarationId = slotDict["declarationId"].AsString();
                var quantity = slotDict["quantity"].AsInt32();

                Dictionary<string, Variant> metadata = null;
                if (slotDict.ContainsKey("metadata"))
                {
                    metadata = new Dictionary<string, Variant>();
                    var metaDict = slotDict["metadata"].AsGodotDictionary();
                    foreach (var metaKey in metaDict.Keys)
                        metadata[metaKey.AsString()] = metaDict[metaKey];
                }

                container.SetSlot(i, new ItemStack(declarationId, quantity, metadata));
            }
        }
    }
}
