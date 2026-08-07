using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Effect Prefabs")]
    public GameObject WarriorSlashVFX;
    public GameObject MageFireballVFX;
    public GameObject LevelUpVFX;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySkillVFX(int skillId, Vector3 targetPosition)
    {
        GameObject prefabToSpawn = null;
        switch (skillId)
        {
            case 1:
                prefabToSpawn = WarriorSlashVFX;
                break;
            case 2:
                prefabToSpawn = MageFireballVFX;
                break;
            default:
                prefabToSpawn = WarriorSlashVFX;
                break;
        }

        if (prefabToSpawn != null)
        {
            GameObject vfxObj = Instantiate(prefabToSpawn, targetPosition, Quaternion.identity);
            Destroy(vfxObj, 3.0f);
        }
    }

    public void PlayLevelUpVFX(Vector3 playerPosition)
    {
        if (LevelUpVFX != null)
        {
            GameObject vfxObj = Instantiate(LevelUpVFX, playerPosition, Quaternion.identity);
            Destroy(vfxObj, 4.0f);
        }
    }
}
