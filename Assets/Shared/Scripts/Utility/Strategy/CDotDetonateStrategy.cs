using UnityEngine;

/// <summary>
/// 대상에게 걸려 있는 지속 피해의 남은 총 피해량을 즉시 폭발시키기
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/DotDetonateStrategy", fileName = "DotDetonateStrategy_")]
public class CDotDetonateStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null) return;

        if (!context.IsHit)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나가 도트뎀 폭발에 실패했습니다.");
            return;
        }

        int burstDamage = context.Target.DetonateAllDots();

        if (burstDamage <= 0)
        {
            CPrint.Log($"[{effectData.Name}] : [{context.Target.name}] 에게 걸려 있는 지속 피해가 없어 폭발 데미지가 0입니다.");
            return;
        }

        CPrint.Log($"[{context.Caster.name}] 의 <{effectData.Name}> 발동! [{context.Target.name}] 의 남은 도트 데미지를 폭발시켜 [{burstDamage}] 데미지를 입힙니다.");
        context.Target.TakeDamage(burstDamage);
    }
}