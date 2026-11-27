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

        CEntity defender = ResolveDefender(context);
        CEnemyGuardContext.SetDefender(context, defender);
        context.IsCrit = false;
        if (!CheckHit(context, defender))
        {
            context.IsHit = false;
            defender.ReportMissedDamage(context.Caster);
            return;
        }

        bool hasCombo = _criticalOnCombo && defender.TokenManager.HasToken(EEffectType.Combo);
        bool hasCriticalToken = context.ConsumeCasterTokenOnce(EEffectType.Crit);
        context.Caster.TryGetComponent(out CDenialLock denialLock);
        int criticalChance = effectData.BaseChance;
        if (denialLock != null) criticalChance += denialLock.CriticalChanceBonus;
        context.IsCrit = hasCriticalToken || hasCombo || CHeroCriticalState.Roll(context.Caster, criticalChance);

        float damage = effectData.RollValue(context.IsCrit);
        if (denialLock != null) damage *= 1.0f + denialLock.DamageBonusPercent * 0.01f;
        if (context.IsCrit) damage *= 1.5f;
        if (context.ConsumeCasterTokenOnce(EEffectType.Strength)) damage *= 1.5f;
        if (context.ConsumeCasterTokenOnce(EEffectType.Weak)) damage *= 0.5f;

        if (defender.TokenManager.ConsumeToken(EEffectType.BlockPlus))
        {
            damage *= 0.25f;
        }
        else if (defender.TokenManager.ConsumeToken(EEffectType.Block))
        {
            damage *= 0.5f;
        }

        if (defender.TokenManager.ConsumeToken(EEffectType.Vulnerable)) damage *= 1.5f;
        if (hasCombo) defender.TokenManager.ConsumeToken(EEffectType.Combo);

        defender.ApplyDamageEffect(context.Caster, Mathf.RoundToInt(damage), context.IsCrit);
        if (denialLock != null) denialLock.ApplyInheritedOnHitEffect(CEnemyGuardContext.GetEffectContext(context));
    }

    private CEntity ResolveDefender(CEffectContext context)
    {
        CEntity target = CEnemyGuardContext.GetEffectContext(context).Target;
        if (target != context.Target) return target;
        if (!target.TokenManager.HasToken(EEffectType.Guard)) return target;

        CEntity protector = target.GuardProtector;
        if (protector == null || protector == target || protector.IsDead
            || protector.TokenManager == null || protector.TokenManager.HasToken(EEffectType.Stun))
        {
            target.SetGuardProtector(null);
            return target;
        }

        target.TokenManager.ConsumeToken(EEffectType.Guard);
        return protector;
    }

    private bool CheckHit(CEffectContext context, CEntity defender)
    {
        if (context.ConsumeCasterTokenOnce(EEffectType.Blind) && Random.Range(1, 101) <= 50)
        {
            return false;
        }

        if (defender.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            return Random.Range(1, 101) > 75;
        }

        if (defender.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            return Random.Range(1, 101) > 50;
        }

        return true;
    }
}
