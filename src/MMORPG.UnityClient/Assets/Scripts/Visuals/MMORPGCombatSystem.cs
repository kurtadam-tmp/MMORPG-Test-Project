using UnityEngine;

public class MMORPGCombatSystem : MonoBehaviour
{
    [Header("Universal Combat Settings")]
    public float AttackRange = 10.0f;
    public int BaseDamage = 35;
    public float CriticalHitRate = 0.25f; // 25% Crit chance

    public void ExecuteAttack()
    {
        // Find nearest IDamageable enemy in range
        IDamageable nearestTarget = FindNearestTarget();
        if (nearestTarget == null)
        {
            Debug.Log("[Combat] No target in attack range.");
            return;
        }

        Component targetComp = nearestTarget as Component;
        if (targetComp == null) return;

        // Calculate Clean Damage & Critical Hit
        bool isCrit = Random.value <= CriticalHitRate;
        int damage = isCrit ? Mathf.RoundToInt(BaseDamage * 2.5f) : BaseDamage;

        // Deal Damage to Target
        nearestTarget.TakeDamage(damage, isCrit, transform.position);

        Debug.Log($"[Combat] Attacked '{nearestTarget.EntityName}'! Damage: {damage} (Crit: {isCrit})");
    }

    private IDamageable FindNearestTarget()
    {
        IDamageable nearest = null;
        float minDistance = AttackRange;

        // Find all UniversalCombatEntity objects in scene
#if UNITY_2023_1_OR_NEWER
        var entities = Object.FindObjectsByType<UniversalCombatEntity>(FindObjectsSortMode.None);
#else
        var entities = Object.FindObjectsOfType<UniversalCombatEntity>();
#endif

        foreach (var entity in entities)
        {
            if (entity.IsDead || entity.gameObject == gameObject) continue;

            float dist = Vector3.Distance(transform.position, entity.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = entity;
            }
        }

        // Also fallback check for MMORPGBossVisualizer
        if (nearest == null)
        {
#if UNITY_2023_1_OR_NEWER
            var bosses = Object.FindObjectsByType<MMORPGBossVisualizer>(FindObjectsSortMode.None);
#else
            var bosses = Object.FindObjectsOfType<MMORPGBossVisualizer>();
#endif
            foreach (var boss in bosses)
            {
                float dist = Vector3.Distance(transform.position, boss.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    // Wrap boss hit
                    boss.TakeDamage(Mathf.RoundToInt(BaseDamage * 2.5f));
                    DamageTextManager.Instance?.SpawnDamageText(boss.transform.position + Vector3.up * 3f, BaseDamage * 2, true);
                    VFXManager.Instance?.PlaySkillVFX(1, boss.transform.position);
                    break;
                }
            }
        }

        return nearest;
    }
}
