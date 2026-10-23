using System.Collections;
using UnityEngine;

/// <summary>
/// 대상의 체력을 회복
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/HealStrategy", fileName = "HealStrategy_")]
public class CHealStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        int healAmount = effectData.RollValue(context.IsCrit); // CEffectSO의 Damage 수치를 회복량으로 사용

        if (context.IsCrit)
        {
            healAmount = Mathf.RoundToInt(healAmount * 1.5f);
            CPrint.Log($"[{effectData.Name}] : 크리티컬 회복 발동");
        }

        // 지금은 TakeDamage에 음수로 처리중인데 나중에 CEntity Heal() 함수를 파서 할 수도..?
        context.Target.TakeDamage(-healAmount);
        CPrint.Log($"[{context.Caster.name}]가 [{context.Target.name}]의 체력을 [{healAmount}] 회복시켰습니다.");
    }
}
