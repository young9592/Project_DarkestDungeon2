using UnityEngine;

/// <summary>
/// 대상의 긍정 버프 토큰을 빼앗아 시전자에게 부여시키기
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/StealTokenStrategy", fileName = "StealTokenStrategy_")]
public class CStealTokenStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        if (!context.IsHit)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나가 토큰 강탈에 실패했습니다.");
            return;
        }

        int maxStealCount = effectData.Damage > 0 ? effectData.Damage : 2;

        int stolenCount = context.Target.TokenManager.StealPositiveTokensTo(context.Caster.TokenManager, maxStealCount);

        if (stolenCount > 0)
        {
            CPrint.Log($"[{context.Caster.name}]가 [{context.Target.name}]의 버프 토큰 {stolenCount}종류를 강탈했습니다.");
        }
        else
        {
            CPrint.Log($"[{context.Target.name}] 에게 강탈할 버프 토큰이 없습니다.");
        }
    }
}
