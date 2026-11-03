using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Utility Accuracy", fileName = "HeroUtilityAccuracyStrategy_")]
public class CHeroUtilityAccuracyStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit || context.Caster == null
            || context.Target == null || context.Caster == context.Target
            || context.Caster.TokenManager == null) return;

        if (context.Caster.TokenManager.ConsumeToken(EEffectType.Blind)
            && Random.Range(1, 101) <= 50)
        {
            context.IsHit = false;
            context.IsCrit = false;
            return;
        }

        if (effectData.BaseChance <= 0)
        {
            context.IsCrit = false;
            return;
        }

        context.IsCrit = context.Caster.TokenManager.ConsumeToken(EEffectType.Crit)
            || CHeroCriticalState.Roll(context.Caster, effectData.BaseChance);
    }
}
