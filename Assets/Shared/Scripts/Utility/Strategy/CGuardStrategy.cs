using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/GuardStrategy", fileName = "GuardStrategy_")]
public class CGuardStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        if (context.Target == context.Caster)
        {
            CPrint.Log($"[{effectData.Name}] : 자기 자신은 수호할 수 없습니다.");
            return;
        }

        int stackAmount = Mathf.Max(1, effectData.Damage);
        context.Target.TokenManager.AddToken(EEffectType.Guard, stackAmount);
        context.Target.SetGuardProtector(context.Caster);

        CPrint.Log($"[수호 설정] [{context.Caster.name}] 가 [{context.Target.name}]을(를) {stackAmount}회 수호합니다.");
    }
}
