using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Positive Tokens", fileName = "HeroPositiveTokenStrategy_")]
public class CHeroPositiveTokenStrategy : CEffectStrategy
{
    [SerializeField] private bool _stealTokens;

    private static readonly EEffectType[] _positiveTokens =
    {
        EEffectType.Crit, EEffectType.Riposte, EEffectType.Dodge, EEffectType.DodgePlus,
        EEffectType.Block, EEffectType.BlockPlus, EEffectType.Guard,
        EEffectType.Strength, EEffectType.Speed, EEffectType.Stealth
    };

    public bool StealTokens { get { return _stealTokens; } }

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || effectData == null || context.Caster == null
            || context.Target == null || context.Caster == context.Target
            || context.Caster.IsDead || context.Target.IsDead
            || !context.Caster.isActiveAndEnabled || !context.Target.isActiveAndEnabled
            || context.Caster.TokenManager == null || context.Target.TokenManager == null) return;

        int amount = Mathf.Max(0, effectData.Damage);
        for (int i = 0; i < amount; i++)
        {
            int count = 0;
            for (int j = 0; j < _positiveTokens.Length; j++)
            {
                if (context.Target.TokenManager.HasToken(_positiveTokens[j])) count++;
            }
            if (count == 0) return;

            int selected = Random.Range(0, count);
            for (int j = 0; j < _positiveTokens.Length; j++)
            {
                EEffectType token = _positiveTokens[j];
                if (!context.Target.TokenManager.HasToken(token)) continue;
                if (selected > 0)
                {
                    selected--;
                    continue;
                }

                if (context.Target.TokenManager.ConsumeToken(token) && _stealTokens)
                {
                    context.Caster.TokenManager.AddToken(token);
                }
                break;
            }
        }
    }
}
