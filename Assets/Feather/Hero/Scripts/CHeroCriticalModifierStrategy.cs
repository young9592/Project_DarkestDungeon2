using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Critical Modifier", fileName = "HeroCriticalModifierStrategy_")]
public class CHeroCriticalModifierStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || effectData == null || context.Target == null
            || context.Target.IsDead || !context.Target.isActiveAndEnabled) return;

        if (effectData.Damage < 0)
        {
            int resistance = context.Target.EntityData != null ? context.Target.EntityData.Resistance.Debuff : 0;
            int chance = Mathf.Clamp(effectData.BaseChance - resistance, 0, 100);
            if (chance < 100 && Random.Range(1, 101) > chance) return;
        }

        CHeroCriticalState state = context.Target.GetComponent<CHeroCriticalState>();
        if (state == null) state = context.Target.gameObject.AddComponent<CHeroCriticalState>();
        state.AddModifier(effectData.Damage, effectData.Duration, context.Caster == context.Target);
    }
}
