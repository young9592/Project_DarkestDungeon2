using UnityEngine;

/// <summary>
/// 대상의 스트레스를 증가시키거나 회복시킴
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/StressStrategy", fileName = "StressStrategy_")]
public class CStressStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (!context.IsHit && context.Target != context.Caster)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나가 스트레스에 영향이 끼치지 않았습니다.");
            return;
        }

        int stressAmount = effectData.Damage;

        if (effectData.BaseChance < 100)
        {
            int roll = Random.Range(1, 101);
            if (roll > effectData.BaseChance)
            {
                CPrint.Log($"[{effectData.Name}] : 스트레스 부여 실패 ({roll} > {effectData.BaseChance}%)");
                return;
            }
        }

        context.Target.ModifyStress(stressAmount);
    }
}
