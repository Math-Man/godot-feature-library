using Godot;

namespace GodotFeatureLibrary.WorldSpaceUI;

/// <summary>
/// Syncs SubViewport resolution to match mesh aspect ratio, or vice versa.
/// Attach to the MeshInstance3D displaying the viewport.
/// </summary>
[Tool]
public partial class WorldSpaceScreenSync : MeshInstance3D
{
    [Export] public SubViewport TargetViewport { get; set; }
    [Export] public CollisionShape3D ScreenCollider { get; set; }
    [Export] public float ColliderDepth { get; set; } = 0.1f;

    public override void _Ready()
    {
        // Set ViewportTexture at runtime to avoid path invalidation on reimport
        if (!Engine.IsEditorHint() && TargetViewport != null)
        {
            SetupViewportTexture();
        }
    }

    private void SetupViewportTexture()
    {
        // Use GetTexture() which returns a properly connected ViewportTexture
        var viewportTex = TargetViewport.GetTexture();

        var material = GetActiveMaterial(0);
        if (material is StandardMaterial3D stdMat)
        {
            stdMat.AlbedoTexture = viewportTex;
        }
        else if (material is ShaderMaterial shaderMat)
        {
            shaderMat.SetShaderParameter("albedoTex", viewportTex);
        }
    }

    [ExportGroup("Mesh → Viewport")]
    [Export(PropertyHint.Range, "100,2000,10")]
    public int BaseViewportHeight { get; set; } = 540;

    [ExportToolButton("Sync Viewport to Mesh")]
    private Callable SyncViewportButton => Callable.From(SyncViewportToMesh);

    [ExportGroup("Viewport → Mesh")]
    [Export] public float BaseMeshHeight { get; set; } = 1f;

    [ExportToolButton("Sync Mesh to Viewport")]
    private Callable SyncMeshButton => Callable.From(SyncMeshToViewport);

    /// <summary>
    /// Adjusts viewport resolution to match mesh aspect ratio.
    /// Use this when mesh size is fixed (from Blender).
    /// </summary>
    public void SyncViewportToMesh()
    {
        if (TargetViewport == null || Mesh is not QuadMesh quad) return;

        var meshSize = quad.Size;
        var aspectRatio = meshSize.X / meshSize.Y;
        var newWidth = Mathf.RoundToInt(BaseViewportHeight * aspectRatio);

        TargetViewport.Size = new Vector2I(newWidth, BaseViewportHeight);
        SyncCollider(meshSize);

        GD.Print($"[ScreenSync] Set viewport to {TargetViewport.Size} (aspect {aspectRatio:F2})");
    }

    /// <summary>
    /// Adjusts mesh size to match viewport aspect ratio.
    /// Use this when viewport resolution is fixed.
    /// </summary>
    public void SyncMeshToViewport()
    {
        if (TargetViewport == null || Mesh is not QuadMesh quad) return;

        var viewportSize = TargetViewport.Size;
        var aspectRatio = (float)viewportSize.X / viewportSize.Y;

        quad.Size = new Vector2(BaseMeshHeight * aspectRatio, BaseMeshHeight);
        SyncCollider(quad.Size);

        GD.Print($"[ScreenSync] Set mesh size to {quad.Size} (aspect {aspectRatio:F2})");
    }

    private void SyncCollider(Vector2 meshSize)
    {
        if (ScreenCollider == null) return;

        if (ScreenCollider.Shape is not BoxShape3D box)
        {
            box = new BoxShape3D();
            ScreenCollider.Shape = box;
        }

        box.Size = new Vector3(meshSize.X, meshSize.Y, ColliderDepth);
        GD.Print($"[ScreenSync] Set collider size to {box.Size}");
    }
}
