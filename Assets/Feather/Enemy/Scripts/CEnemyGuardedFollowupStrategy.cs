using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Enemy/Guarded Followup", fileName = "EnemyGuardedFollowupStrategy_")]
public class CEnemyGuardedFollowupStrategy : CEffectStrategy
{
    [SerializeField] private CEffectStrategy _strategy;

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || _strategy == null || _strategy == this) return;
        CEffectContext resolved = CEnemyGuardContext.GetEffectContext(context);
        if (resolved.Target == null || !resolved.Target.isActiveAndEnabled || resolved.Target.IsDead) return;
        _strategy.Apply(resolved, effectData);
    }
}
