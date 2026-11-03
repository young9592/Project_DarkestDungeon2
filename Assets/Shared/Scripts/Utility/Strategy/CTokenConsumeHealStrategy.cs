using UnityEngine;

/// <summary>
/// 대상 모든 토큰 소모하고, 소모한 토큰 개수 x 기본 Damage 만큼 체려 회복시킴
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/TokenConsumeHealStrategy", fileName = "TokenConsumeHealStrategy_")]
public class CTokenConsumeHealStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null || context.Target.TokenManager == null) return;

        int consumedCount = context.Target.TokenManager.ConsumeAllTokensCount();

        if (consumedCount <= 0)
        {
            CPrint.Log($"[{effectData.Name}] : [{context.Target.name}]에게 소모할 토큰이 없어 회복이 발동하지 않았습니다.");
            return;
        }

        int healPerToken = Mathf.Max(1, effectData.Damage);
        int totalHeal = consumedCount * healPerToken;

        if (context.IsCrit)
        {
            totalHeal = Mathf.RoundToInt(totalHeal * 1.5f);
        }

        CPrint.Log($"[{effectData.Name}] : 토큰 {consumedCount}개 소모 → [{context.Target.name}] 체력 +{totalHeal} 회복");
        context.Target.TakeDamage(-totalHeal);
    }
}
