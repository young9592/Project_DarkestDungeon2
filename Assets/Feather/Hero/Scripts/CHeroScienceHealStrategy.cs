using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Science Heal", fileName = "HeroScienceHealStrategy_")]
public class CHeroScienceHealStrategy : CEffectStrategy
{
    [SerializeField] private bool _removeNegativeTokens;
    private static readonly EEffectType[] _positiveTokens =
    {
        EEffectType.Crit, EEffectType.Riposte, EEffectType.Dodge, EEffectType.DodgePlus,
        EEffectType.Block, EEffectType.BlockPlus, EEffectType.Guarded,
        EEffectType.Strength, EEffectType.Speed, EEffectType.Stealth
    };
    private static readonly EEffectType[] _negativeTokens =
    {
        EEffectType.Daze, EEffectType.Stun, EEffectType.Blind, EEffectType.Weak,
        EEffectType.Vulnerable, EEffectType.Taunt, EEffectType.Immobilize
    };

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || effectData == null
            || !(context.Target is CPlayer hero) || hero.IsDead || hero.HeroData == null
            || (long)hero.CurrentHp * 2 >= hero.HeroData.MaxHealth) return;

        int percent = 10 + ConsumeTokens(hero.TokenManager, _positiveTokens) * 10;
        if (_removeNegativeTokens) percent += ConsumeTokens(hero.TokenManager, _negativeTokens) * 5;
        int amount = Mathf.RoundToInt(hero.HeroData.MaxHealth * (percent / 100f));
        if (amount > 0) hero.TakeDamage(-amount);
    }

    private int ConsumeTokens(CTokenManager manager, EEffectType[] types)
    {
        int count = 0;
        for (int i = 0; i < types.Length; i++)
        {
            while (manager.ConsumeToken(types[i])) count++;
        }
        return count;
    }
}
