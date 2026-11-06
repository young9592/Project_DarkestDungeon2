using UnityEngine;

/// <summary>
/// 대상의 특정 토큰 제거하거나, 버프 / 디버프 토큰을 일관 해제시킴
/// ex : 노상강도 - 추적 사격, 중보병 - 지휘, 포효
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/RemoveTokenStrategy", fileName = "RemoveTokenStrategy_")]
public class CRemoveTokenStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null || context.Target.TokenManager == null) return;

        if (!context.IsHit && context.Target != context.Caster)
        {
            CPrint.Log($"[{effectData.Name}] : 선행 공격이 빗나가 토큰 제거 실패하였습니다.");
            return;
        }

        EEffectType targetToken = effectData.TokenType;

        if (targetToken != EEffectType.None)
        {
            int removed = context.Target.TokenManager.RemoveTokenAll(targetToken);
            if (removed > 0)
            {
                CPrint.Log($"[{context.Target.name}] 의 [{targetToken}] 토큰 {removed}개를 제거했습니다.");
            }
            else
            {
                CPrint.Log($"[{context.Target.name}] 에게 제거할 [{targetToken}] 토큰이 없습니다.");
            }
            return;
        }

        // TokenType이 None일시 같은 진영이면 디버프 정화 / 다른 진영일시 버프 디스펠
        bool isAllyOrSelf = (context.Caster != null && context.Caster.EntityType == context.Target.EntityType)
                            || effectData.EffectTarget == EEffectTarget.Self;

        bool removePositive = !isAllyOrSelf;
        int totalRemoved = context.Target.TokenManager.RemoveTokensByCategory(removePositive);

        string categoryName = removePositive ? "버프" : "디버프";
        CPrint.Log($"[{context.Target.name}]의 {categoryName} 토큰 총 {totalRemoved}개를 해제했습니다.");
    }
}
