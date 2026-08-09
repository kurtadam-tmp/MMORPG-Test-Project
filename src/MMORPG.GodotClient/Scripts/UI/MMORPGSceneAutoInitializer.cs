using Godot;

public partial class MMORPGSceneAutoInitializer : Node
{
    public override void _Ready()
    {
        GD.Print("[MMORPGSceneAutoInitializer] Verifying Godot MMORPG World Hierarchy...");

        Node3D? parent = GetParent<Node3D>();
        if (parent == null) return;

        // Ensure nodes are added deferred if missing
        EnsureNodeExists<DirectionalLight3D>(parent, "DirectionalSun", () =>
        {
            DirectionalLight3D sun = new DirectionalLight3D();
            sun.Name = "DirectionalSun";
            sun.RotationDegrees = new Vector3(-50f, -30f, 0f);
            sun.LightEnergy = 1.2f;
            return sun;
        });

        EnsureNodeExists<MeshInstance3D>(parent, "GroundPlane", () =>
        {
            MeshInstance3D ground = new MeshInstance3D();
            ground.Name = "GroundPlane";
            ground.Mesh = new PlaneMesh { Size = new Vector2(60f, 60f) };
            StandardMaterial3D groundMat = new StandardMaterial3D { AlbedoColor = new Color(0.12f, 0.18f, 0.25f) };
            ground.MaterialOverride = groundMat;
            return ground;
        });

        GD.Print(" ✅ Godot 2.5D World Scene Hierarchy Verified!");
    }

    private void EnsureNodeExists<T>(Node parent, string nodeName, System.Func<T> factory) where T : Node
    {
        if (!parent.HasNode(nodeName))
        {
            T newChild = factory();
            parent.CallDeferred(Node.MethodName.AddChild, newChild);
        }
    }
}
