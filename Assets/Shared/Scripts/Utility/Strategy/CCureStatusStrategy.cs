using UnityEngine;

/// <summary>
/// 대상에게 걸려있는 지속 피해 상태이상 제거
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/CureStatusStrategy", fileName = "CureStatusStrategy_")]
public class CCureStatusStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null) return;

        EEffectType cureTargetType = effectData.TokenType;

        context.Target.CureStatusEffect(cureTargetType);
    }
}
