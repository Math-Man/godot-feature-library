using Godot;

namespace GodotFeatureLibrary.Overlay;

/// <summary>
/// Static AABB utilities for projecting 3D bounding boxes to screen space.
/// </summary>
public static class OverlayGeometry
{
    public static Aabb? ComputeGlobalAabb(Node3D target)
    {
        Aabb? result = null;
        CollectAabb(target, ref result);
        return result;
    }

    public static Vector3[] GetAabbCorners(Aabb aabb)
    {
        var pos = aabb.Position;
        var size = aabb.Size;
        return new[]
        {
            pos,
            pos + new Vector3(size.X, 0, 0),
            pos + new Vector3(0, size.Y, 0),
            pos + new Vector3(0, 0, size.Z),
            pos + new Vector3(size.X, size.Y, 0),
            pos + new Vector3(size.X, 0, size.Z),
            pos + new Vector3(0, size.Y, size.Z),
            pos + size,
        };
    }

    private static void CollectAabb(Node node, ref Aabb? result)
    {
        if (node is VisualInstance3D visual)
        {
            var localAabb = visual.GetAabb();
            var corners = GetAabbCorners(localAabb);

            // Transform corners to global space
            var globalAabb = new Aabb(visual.GlobalTransform * corners[0], Vector3.Zero);
            for (int i = 1; i < 8; i++)
                globalAabb = globalAabb.Expand(visual.GlobalTransform * corners[i]);

            result = result?.Merge(globalAabb) ?? globalAabb;
        }

        foreach (var child in node.GetChildren())
            CollectAabb(child, ref result);
    }
}
