using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.Overlay;

public partial class OverlayService
{
    private void ProcessLineGroups(Camera3D camera, float delta)
    {
        for (int i = _lineGroups.Count - 1; i >= 0; i--)
        {
            var group = _lineGroups[i];
            group.Elapsed += delta;

            if (camera != null)
            {
                // Project all world points to screen
                var projected = new Vector2[group.ToWorld.Length];
                bool allValid = true;
                for (int j = 0; j < group.ToWorld.Length; j++)
                {
                    if (camera.IsPositionBehind(group.ToWorld[j])
                        || camera.GlobalPosition.DistanceSquaredTo(group.ToWorld[j]) < 0.001f)
                    {
                        allValid = false;
                        break;
                    }

                    projected[j] = camera.UnprojectPosition(group.ToWorld[j]);
                }

                if (!allValid)
                {
                    foreach (var l in group.Instances) l.Visible = false;
                }
                else
                {
                    int count = group.Instances.Count;

                    // Deduplicate world points that overlap in screen space
                    // (e.g. front/back AABB corners). Keep the one nearest to camera.
                    const float dedupeThreshold = 10f; // pixels
                    var unique = new List<(int idx, Vector2 pos)>(projected.Length);
                    for (int j = 0; j < projected.Length; j++)
                    {
                        bool isDuplicate = false;
                        for (int k = 0; k < unique.Count; k++)
                        {
                            if (projected[j].DistanceSquaredTo(unique[k].pos) < dedupeThreshold * dedupeThreshold)
                            {
                                // Keep the closer one to camera
                                float distJ = camera.GlobalPosition.DistanceSquaredTo(group.ToWorld[j]);
                                float distK = camera.GlobalPosition.DistanceSquaredTo(group.ToWorld[unique[k].idx]);
                                if (distJ < distK)
                                    unique[k] = (j, projected[j]);
                                isDuplicate = true;
                                break;
                            }
                        }

                        if (!isDuplicate) unique.Add((j, projected[j]));
                    }

                    // Build deduplicated projection array
                    var filteredProjected = new Vector2[unique.Count];
                    for (int j = 0; j < unique.Count; j++)
                        filteredProjected[j] = unique[j].pos;

                    // Find optimal 1:1 assignment
                    var bestAssignment = new int[count];
                    var currentAssignment = new int[count];
                    var used = new bool[filteredProjected.Length];
                    float bestCost = float.MaxValue;

                    FindBestAssignment(group.FromScreen, filteredProjected, currentAssignment, used, 0, 0f,
                        ref bestCost, ref bestAssignment);

                    for (int j = 0; j < count; j++)
                    {
                        group.Instances[j].SetPointPosition(0, group.FromScreen[j]);
                        // Map back to original projected index
                        int originalIdx = unique[bestAssignment[j]].idx;
                        group.Instances[j].SetPointPosition(1, projected[originalIdx]);
                        group.Instances[j].Visible = true;
                    }
                }
            }
            else
            {
                foreach (var l in group.Instances) l.Visible = false;
            }

            // Fade
            float groupAlpha = 1f;
            if (group.FadeIn > 0f && group.Elapsed < group.FadeIn)
                groupAlpha = group.Elapsed / group.FadeIn;
            if (group.Duration > 0f && group.FadeOut > 0f)
            {
                float timeLeft = group.Duration - group.Elapsed;
                if (timeLeft < group.FadeOut)
                    groupAlpha = Mathf.Min(groupAlpha, timeLeft / group.FadeOut);
            }

            var mod = new Color(1f, 1f, 1f, Mathf.Clamp(groupAlpha, 0f, 1f));
            foreach (var l in group.Instances) l.Modulate = mod;

            if (group.Duration > 0f && group.Elapsed >= group.Duration)
            {
                RemoveLineGroupAt(i);
            }
        }
    }

    /// <summary>
    /// Branch-and-bound search for optimal 1:1 point assignment minimizing total distance.
    /// </summary>
    private static void FindBestAssignment(Vector2[] from, Vector2[] to, int[] current, bool[] used, int depth,
        float cost, ref float bestCost, ref int[] bestResult)
    {
        if (depth == from.Length)
        {
            if (cost < bestCost)
            {
                bestCost = cost;
                System.Array.Copy(current, bestResult, from.Length);
            }

            return;
        }

        for (int k = 0; k < to.Length; k++)
        {
            if (used[k]) continue;
            float newCost = cost + from[depth].DistanceSquaredTo(to[k]);
            if (newCost >= bestCost) continue; // prune
            current[depth] = k;
            used[k] = true;
            FindBestAssignment(from, to, current, used, depth + 1, newCost, ref bestCost, ref bestResult);
            used[k] = false;
        }
    }

    private void RemoveLineGroupAt(int index)
    {
        var group = _lineGroups[index];
        foreach (var l in group.Instances) l.QueueFree();
        _lineGroups.RemoveAt(index);
    }

    private class ActiveLineGroup
    {
        public string Id;
        public List<Line2D> Instances;
        public Vector2[] FromScreen;
        public Vector3[] ToWorld;
        public float Duration;
        public float Elapsed;
        public float FadeIn;
        public float FadeOut;
    }
}
