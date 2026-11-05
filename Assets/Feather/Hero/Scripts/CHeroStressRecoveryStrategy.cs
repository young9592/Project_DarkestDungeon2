using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Stress Recovery", fileName = "HeroStressRecoveryStrategy_")]
public class CHeroStressRecoveryStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !(context.Target is CPlayer hero)
            || hero.IsDead || hero.CurrentStress < 5 || (!context.IsHit && context.Caster != hero)) return;

        hero.ModifyStress(-Mathf.Abs(effectData.Damage));
    }
}
