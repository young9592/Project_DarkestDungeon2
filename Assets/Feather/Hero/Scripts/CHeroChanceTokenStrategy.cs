using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Chance Token", fileName = "HeroChanceTokenStrategy_")]
public class CHeroChanceTokenStrategy : CAddTokenStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit) return;

        int chance = context.IsCrit ? 100 : Mathf.Clamp(effectData.BaseChance, 0, 100);
        if (Random.Range(1, 101) > chance) return;
        base.Apply(context, effectData);
    }
}
