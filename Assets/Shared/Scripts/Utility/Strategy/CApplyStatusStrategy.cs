using UnityEngine;

/// <summary>
/// 상태이상을 대상에게 부여할 때 사용함
/// 저항 계산을 한 후, 대상에게 상태이상을 적용하는 로직
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/ApplyStatusStrategy", fileName = "ApplyStatusStrategy_")]
public class CApplyStatusStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (!context.IsHit)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나가서 상태이상 부여에 실패하였습니다.");
            return;
        }

        int targetResistance = GetTargetResistance(context.Target, effectData.EffectType);

        int rawChance = effectData.BaseChance - targetResistance;
        int finalChance = Mathf.Clamp(rawChance, 0, 100);

        CPrint.Log($"[{effectData.Name}] 부여 시도 \n " +
            $"기본 확률 : {effectData.BaseChance}% - 대상 저항력 : {targetResistance}% = 최종 확률 : {finalChance}%"); // 결과 확인용

        int randomRoll = Random.Range(1, 101);

        if (randomRoll <= finalChance)
        {
            CPrint.Log($"[{effectData.Name}] 저항 뚫기 성공 \n" +
                $"(주사위: {randomRoll} <= 확률 : {finalChance})"); // 결과 확인용

            CEffect statusEffect = new CEffect(effectData);
            context.Target.ApplyStatusEffect(statusEffect);
        }
        else
        {
            CPrint.Log($"[{effectData.Name}] 대상이 저항했습니다 \n" +
                $"(주사위: {randomRoll} > 확률 : {finalChance})"); // 결과 확인용

            // TODO : UI에서 저항함 텍스트 플로팅
        }
    }

    /// <summary>
    /// 상태이상 타입에 맞춰서 타겟 저항력 스텟 가져오기
    /// </summary>
    private int GetTargetResistance(CEntity target, EEffectType effectType)
    {
        if (target == null || target.EntityData == null) return 0;

        var res = target.EntityData.Resistance;

        switch (effectType)
        {
            case EEffectType.Bleed: return res.Bleed;
            case EEffectType.Blight: return res.Blight;
            case EEffectType.Burn: return res.Burn;
            case EEffectType.Stun:
            case EEffectType.Daze: return res.Stun;
            case EEffectType.Weak:
            case EEffectType.Vulnerable:
            case EEffectType.Blind: return res.Debuff;
            default:                return 0;

        }
    }
}
