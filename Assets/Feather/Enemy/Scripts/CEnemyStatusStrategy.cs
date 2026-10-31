using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Enemy/Status", fileName = "EnemyStatusStrategy_")]
public class CEnemyStatusStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit
            || context.Target == null || context.Target.IsDead
            || !context.Target.isActiveAndEnabled || context.Target.EntityData == null) return;

        CEntityDataSO.CResistanceStats stats = context.Target.EntityData.Resistance;
        int resistance;
        switch (effectData.EffectType)
        {
            case EEffectType.Bleed:
                resistance = stats.Bleed;
                break;
            case EEffectType.Blight:
                resistance = stats.Blight;
                break;
            case EEffectType.Burn:
                resistance = stats.Burn;
                break;
            default:
                return;
        }

        int piercing = context.IsCrit ? 20 : 0;
        int chance = Mathf.Clamp(effectData.BaseChance + piercing - resistance, 0, 100);
        if (Random.Range(1, 101) > chance) return;

        context.Target.ApplyStatusEffect(new CEffect(effectData));
    }
}
