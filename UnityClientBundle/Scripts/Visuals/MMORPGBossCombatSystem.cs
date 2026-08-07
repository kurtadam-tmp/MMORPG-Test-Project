using UnityEngine;

public class MMORPGBossCombatSystem : MonoBehaviour
{
    [Header("Combat Settings")]
    public float AttackRange = 15.0f;
    public int BaseSpellDamage = 3500;
    public float CriticalHitRate = 0.30f; // 30% Crit chance

    private MMORPGBossVisualizer _targetBoss;

    private void Start()
    {
        // Auto-find World Boss Ignis in scene
#if UNITY_2023_1_OR_NEWER
        _targetBoss = Object.FindFirstObjectByType<MMORPGBossVisualizer>();
#else
        _targetBoss = Object.FindObjectOfType<MMORPGBossVisualizer>();
#endif
    }

    public void ExecuteSkillAttack()
    {
        if (_targetBoss == null)
        {
#if UNITY_2023_1_OR_NEWER
            _targetBoss = Object.FindFirstObjectByType<MMORPGBossVisualizer>();
#else
            _targetBoss = Object.FindObjectOfType<MMORPGBossVisualizer>();
#endif
        }

        if (_targetBoss == null) return;

        float distance = Vector3.Distance(transform.position, _targetBoss.transform.position);
        if (distance <= AttackRange)
        {
            // Calculate Damage & Critical Hit
            bool isCrit = Random.value <= CriticalHitRate;
            int damage = isCrit ? Mathf.RoundToInt(BaseSpellDamage * 2.2f) : BaseSpellDamage;

            // Apply Damage to World Boss Ignis
            _targetBoss.TakeDamage(damage);

            // Trigger Floating 3D Damage Text (worldPosition, damageAmount, isCritical)
            Vector3 spawnPos = _targetBoss.transform.position + new Vector3(Random.Range(-1f, 1f), 3.5f, Random.Range(-1f, 1f));
            DamageTextManager.Instance?.SpawnDamageText(spawnPos, damage, isCrit);

            // Trigger Spell VFX Effect
            VFXManager.Instance?.PlaySkillVFX(1, _targetBoss.transform.position);

            Debug.Log($"[Boss Combat] Skill Attack landed on {_targetBoss.BossName}! Damage: {damage} (Crit: {isCrit})");
        }
        else
        {
            Debug.LogWarning($"[Boss Combat] Boss {_targetBoss.BossName} is out of attack range! Distance: {distance:F1}m (Max: {AttackRange}m)");
        }
    }
}
