using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Heal", fileName = "HeroHealStrategy_")]
public class CHeroHealStrategy : CHealStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null || effectData == null)
        {
            return;
        }

        base.Apply(context, effectData);
    }
}
