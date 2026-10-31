using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Enemy/Token", fileName = "EnemyTokenStrategy_")]
public class CEnemyTokenStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit
            || context.Target == null || context.Target.IsDead
            || !context.Target.isActiveAndEnabled || context.Target.TokenManager == null) return;

        EEffectType token = effectData.TokenType != EEffectType.None
            ? effectData.TokenType : effectData.EffectType;
        int resistance = GetResistance(context.Target, token);
        int piercing = context.IsCrit ? 20 : 0;
        int chance = Mathf.Clamp(effectData.BaseChance + piercing - resistance, 0, 100);
        if (Random.Range(1, 101) > chance) return;

        context.Target.TokenManager.AddToken(token, Mathf.Max(1, effectData.Damage));
    }

    private int GetResistance(CEntity target, EEffectType token)
    {
        if (target.EntityData == null) return 0;

        CEntityDataSO.CResistanceStats resistance = target.EntityData.Resistance;
        switch (token)
        {
            case EEffectType.Stun:
            case EEffectType.Daze:
                return resistance.Stun;
            case EEffectType.Blind:
            case EEffectType.Weak:
            case EEffectType.Vulnerable:
                return resistance.Debuff;
            case EEffectType.Immobilize:
                return resistance.Move;
            default:
                return 0;
        }
    }
}
