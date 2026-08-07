using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MMORPGSceneAutoInitializer : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[MMORPGSceneAutoInitializer] Initializing Complete MMORPG World Zone Scene Hierarchy...");

        // 1. Setup Main Camera & Directional Light
        SetupEnvironmentLightAndCamera();

        // 2. Setup Player Visualizer (Hero Avatar & Movement)
        SetupPlayer();

        // 3. Setup World Boss (Inferno Dragon Ignis)
        SetupWorldBoss();

        // 4. Setup Zone Portal (Whisperwood Glen -> Shadowfen Swamps)
        SetupZonePortal();

        // 5. Setup Master UI Canvas & Controllers
        SetupMasterUI();

        // 6. Setup VFX Manager
        SetupVFXManager();
    }

    private void SetupEnvironmentLightAndCamera()
    {
        if (RenderSettings.sun == null)
        {
            Light sunLight = FindFirstObjectByType<Light>();
            if (sunLight == null)
            {
                GameObject lightObj = new GameObject("DirectionalLight");
                sunLight = lightObj.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.intensity = 1.2f;
                sunLight.color = new Color(1.0f, 0.95f, 0.85f);
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
        }
    }

    private void SetupPlayer()
    {
        if (FindFirstObjectByType<MMORPGPlayerVisualizer>() == null)
        {
            GameObject playerObj = new GameObject("PlayerCharacter");
            playerObj.transform.position = Vector3.zero;
            playerObj.AddComponent<MMORPGPlayerVisualizer>();
        }
    }

    private void SetupWorldBoss()
    {
        if (FindFirstObjectByType<MMORPGBossVisualizer>() == null)
        {
            GameObject bossObj = new GameObject("WorldBoss_InfernoDragonIgnis");
            bossObj.transform.position = new Vector3(12f, 0f, 12f);
            bossObj.AddComponent<MMORPGBossVisualizer>();
        }
    }

    private void SetupZonePortal()
    {
        if (FindFirstObjectByType<ZonePortalVisualizer>() == null)
        {
            GameObject portalObj = new GameObject("ZonePortal_ShadowfenSwamps");
            portalObj.transform.position = new Vector3(0f, 0f, -8f);
            portalObj.AddComponent<ZonePortalVisualizer>();
        }
    }

    private void SetupMasterUI()
    {
        if (FindFirstObjectByType<MMORPGMasterUIManager>() == null)
        {
            GameObject uiObj = new GameObject("MMORPGMasterCanvas");
            uiObj.AddComponent<MMORPGMasterUIManager>();
            uiObj.AddComponent<HUDUIController>();
            uiObj.AddComponent<InventoryGridUIController>();
            uiObj.AddComponent<ItemEnhancementUIController>();
            uiObj.AddComponent<EquipmentPaperdollUIController>();
            uiObj.AddComponent<MinimapUIController>();
            uiObj.AddComponent<TargetContextMenuUIController>();
        }
    }

    private void SetupVFXManager()
    {
        if (FindFirstObjectByType<VFXManager>() == null)
        {
            GameObject vfxObj = new GameObject("VFXManager");
            vfxObj.AddComponent<VFXManager>();
        }
    }
}
