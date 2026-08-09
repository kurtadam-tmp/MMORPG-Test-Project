using Godot;

public partial class MMORPGSceneAutoInitializer : Node
{
    public override void _Ready()
    {
        GD.Print("[MMORPGSceneAutoInitializer] Initializing Godot MMORPG World Hierarchy...");

        Node3D parent = GetParent<Node3D>();
        if (parent == null) return;

        // 1. Setup Directional Sun Light
        DirectionalLight3D sun = new DirectionalLight3D();
        sun.Name = "DirectionalSun";
        sun.RotationDegrees = new Vector3(-50f, -30f, 0f);
        sun.LightEnergy = 1.2f;
        parent.AddChild(sun);

        // 2. Setup 2.5D Ground Plane
        MeshInstance3D ground = new MeshInstance3D();
        ground.Name = "MMORPG2DGround";
        ground.Mesh = new PlaneMesh { Size = new Vector2(50f, 50f) };
        StandardMaterial3D groundMat = new StandardMaterial3D { AlbedoColor = new Color(0.12f, 0.18f, 0.25f) };
        ground.MaterialOverride = groundMat;
        parent.AddChild(ground);

        // 3. Setup Player Visualizer
        GodotPlayerVisualizer player = new GodotPlayerVisualizer();
        player.Name = "PlayerCharacter";
        parent.AddChild(player);

        // 4. Setup World Boss
        GodotBossVisualizer boss = new GodotBossVisualizer();
        boss.Name = "WorldBoss_Ignis";
        parent.AddChild(boss);

        // 5. Setup Zone Portal
        GodotZonePortalVisualizer portal = new GodotZonePortalVisualizer();
        portal.Name = "ZonePortal_Shadowfen";
        parent.AddChild(portal);

        // 6. Setup Network Client
        MMORPGGodotClient netClient = new MMORPGGodotClient();
        netClient.Name = "MMORPGGodotClient";
        parent.AddChild(netClient);

        // 7. Setup Master UI
        MMORPGMasterGodotUI masterUI = new MMORPGMasterGodotUI();
        masterUI.Name = "MMORPGMasterUI";
        parent.AddChild(masterUI);

        GD.Print(" ✅ Godot 2.5D World Scene Initialized Successfully!");
    }
}
