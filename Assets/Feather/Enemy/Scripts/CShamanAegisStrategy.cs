using UnityEngine;

public class CShamanAegisStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !(context.Caster is CEnemy shaman) || !(context.Target is CEnemy ally)
            || ally == shaman || shaman.IsDead || ally.IsDead) return;

        shaman.TakeDamage(-Mathf.RoundToInt(shaman.EnemyData.MaxHealth * 0.2f));
        ally.TakeDamage(-Mathf.RoundToInt(ally.EnemyData.MaxHealth * 0.2f));
        shaman.TokenManager.AddToken(EEffectType.Dodge, 2);
        ally.TokenManager.AddToken(EEffectType.Guard, 2);
        ally.SetGuardProtector(shaman);
    }
}
