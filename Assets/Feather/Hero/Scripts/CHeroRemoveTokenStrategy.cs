using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Remove Token", fileName = "HeroRemoveTokenStrategy_")]
public class CHeroRemoveTokenStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null || context.Target.TokenManager == null
            || effectData == null || (!context.IsHit && context.Caster != context.Target)) return;

        while (context.Target.TokenManager.ConsumeToken(effectData.TokenType))
        {
        }
    }
}
