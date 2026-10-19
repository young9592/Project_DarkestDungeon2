using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 대상에게 버프, 디버프, 콤보 등 토큰 부여
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/AddTokenStrategy", fileName ="AddTokenStrategy_")]
public class CAddTokenStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        // 디버프성 토큰인데 공격 빗나갈시 부여 취소
        if (!context.IsHit && context.Target != context.Caster)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나가 토큰 부여에 실패했습니다.");
            return;
        }

        EEffectType tokenToApply = effectData.TokenType;
        if (tokenToApply == EEffectType.None)
        {
            tokenToApply = effectData.EffectType;
            // 만일 TokenType을 따로 작성 안했을 시 EffectType 기준
        }

        // Damage 필드를 부여할 토큰 개수(기본 1개)로 하거나 1로 고정
        int amount = Mathf.Max(1, effectData.Damage);
        context.Target.TokenManager.AddToken(tokenToApply, amount);
    }
}
