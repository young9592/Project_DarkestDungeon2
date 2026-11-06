using UnityEngine;

public class CGhoulHorrorStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit || !(context.Target is CPlayer hero)
            || hero.TokenManager == null) return;

        if (context.ConsumeCasterTokenOnce(EEffectType.Blind) && Random.Range(0, 100) < 50)
        {
            context.IsHit = false;
            return;
        }

        int dodgeChance = 0;
        if (hero.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            dodgeChance = 75;
        }
        else if (hero.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            dodgeChance = 50;
        }

        if (dodgeChance > 0 && Random.Range(0, 100) < dodgeChance)
        {
            context.IsHit = false;
            return;
        }

        if (effectData.BaseChance < 100 && Random.Range(0, 100) >= effectData.BaseChance) return;
        hero.ApplyHorror(effectData);
    }
}
