using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Enemy/Damage", fileName = "EnemyDamageStrategy_")]
public class CEnemyDamageStrategy : CEffectStrategy
{
    [Header("Critical")]
    [SerializeField] private bool _criticalOnCombo;

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit
            || context.Caster == null || context.Target == null
            || context.Caster.TokenManager == null || context.Target.TokenManager == null) return;

        context.IsCrit = false;
        if (!CheckHit(context))
        {
            context.IsHit = false;
            return;
        }

        bool hasCombo = _criticalOnCombo && context.Target.TokenManager.HasToken(EEffectType.Combo);
        bool hasCriticalToken = context.ConsumeCasterTokenOnce(EEffectType.Crit);
        context.IsCrit = hasCriticalToken || hasCombo || CHeroCriticalState.Roll(context.Caster, effectData.BaseChance);

        float damage = effectData.RollValue(context.IsCrit);
        if (context.IsCrit) damage *= 1.5f;
        if (context.ConsumeCasterTokenOnce(EEffectType.Strength)) damage *= 1.5f;
        if (context.ConsumeCasterTokenOnce(EEffectType.Weak)) damage *= 0.5f;

        if (context.Target.TokenManager.ConsumeToken(EEffectType.BlockPlus))
        {
            damage *= 0.25f;
        }
        else if (context.Target.TokenManager.ConsumeToken(EEffectType.Block))
        {
            damage *= 0.5f;
        }

        if (context.Target.TokenManager.ConsumeToken(EEffectType.Vulnerable)) damage *= 1.5f;
        if (hasCombo) context.Target.TokenManager.ConsumeToken(EEffectType.Combo);

        context.Target.TakeDamage(Mathf.RoundToInt(damage));
    }

    private bool CheckHit(CEffectContext context)
    {
        if (context.ConsumeCasterTokenOnce(EEffectType.Blind) && Random.Range(1, 101) <= 50)
        {
            return false;
        }

        if (context.Target.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            return Random.Range(1, 101) > 75;
        }

        if (context.Target.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            return Random.Range(1, 101) > 50;
        }

        return true;
    }
}
