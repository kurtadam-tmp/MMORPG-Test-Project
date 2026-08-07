using UnityEngine;

public interface IDamageable
{
    string EntityName { get; }
    int CurrentHp { get; }
    int MaxHp { get; }
    bool IsDead { get; }

    void TakeDamage(int damage, bool isCritical, Vector3 attackerPosition);
}
