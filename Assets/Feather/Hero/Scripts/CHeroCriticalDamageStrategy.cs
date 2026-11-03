using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Critical Damage", fileName = "HeroCriticalDamageStrategy_")]
public class CHeroCriticalDamageStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || effectData == null || context.Caster == null
            || context.Target == null || context.Caster.TokenManager == null
            || context.Target.TokenManager == null) return;

        context.IsCrit = false;
        if (context.ConsumeCasterTokenOnce(EEffectType.Blind) && Random.Range(1, 101) <= 50)
        {
            context.IsHit = false;
            return;
        }

        if (context.Target.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            if (Random.Range(1, 101) <= 75)
            {
                context.IsHit = false;
                return;
            }
        }
        else if (context.Target.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            if (Random.Range(1, 101) <= 75)
            {
                context.IsHit = false;
                return;
            }
        }

        // 치명타 토큰이 없으면 스킬 확률로 한 번만 굴리기
        context.IsCrit = context.ConsumeCasterTokenOnce(EEffectType.Crit)
            || CHeroCriticalState.Roll(context.Caster, effectData.BaseChance);
        float finalDamage = effectData.RollValue(context.IsCrit);
        if (context.IsCrit) finalDamage *= 1.5f;
        if (context.ConsumeCasterTokenOnce(EEffectType.Strength)) finalDamage *= 1.5f;
        if (context.ConsumeCasterTokenOnce(EEffectType.Weak)) finalDamage *= 0.5f;

        if (context.Target.TokenManager.ConsumeToken(EEffectType.BlockPlus)) finalDamage *= 0.25f;
        else if (context.Target.TokenManager.ConsumeToken(EEffectType.Block)) finalDamage *= 0.5f;
        if (context.Target.TokenManager.ConsumeToken(EEffectType.Vulnerable)) finalDamage *= 1.5f;

        context.Target.TakeDamage(Mathf.RoundToInt(finalDamage));
    }
}
