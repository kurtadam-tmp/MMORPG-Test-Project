using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Effect Prefabs")]
    public GameObject WarriorSlashVFX;
    public GameObject MageFireballVFX;
    public GameObject LevelUpVFX;
    public GameObject PortalWarpVFX;
    public GameObject EnhanceSuccessVFX;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnVFX(string vfxName, Vector3 position, Quaternion rotation)
    {
        GameObject prefabToSpawn = vfxName switch
        {
            "VFX_Slash" => WarriorSlashVFX,
            "VFX_Fireball" => MageFireballVFX,
            "VFX_LevelUp" => LevelUpVFX,
            "VFX_PortalWarp" => PortalWarpVFX,
            "VFX_EnhanceSuccess" => EnhanceSuccessVFX,
            _ => WarriorSlashVFX
        };

        if (prefabToSpawn != null)
        {
            GameObject vfxObj = Instantiate(prefabToSpawn, position, rotation);
            Destroy(vfxObj, 3.0f);
        }
        else
        {
            // Debug Fallback Visualizer Effect (Light Pulse)
            GameObject debugEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugEffect.name = $"TempVFX_{vfxName}";
            debugEffect.transform.position = position;
            debugEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            Renderer r = debugEffect.GetComponent<Renderer>();
            if (r != null) r.material.color = Color.yellow;
            Destroy(debugEffect.GetComponent<Collider>());
            Destroy(debugEffect, 1.0f);
        }
    }

    public void PlaySkillVFX(int skillId, Vector3 targetPosition)
    {
        SpawnVFX(skillId == 2 ? "VFX_Fireball" : "VFX_Slash", targetPosition, Quaternion.identity);
    }

    public void PlayLevelUpVFX(Vector3 playerPosition)
    {
        SpawnVFX("VFX_LevelUp", playerPosition, Quaternion.identity);
    }
}
