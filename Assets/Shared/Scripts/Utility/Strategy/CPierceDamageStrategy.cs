using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/PierceDamageStrategy", fileName = "PierceDamageStrategy_")]
public class CPierceDamageStrategy : CDamageStrategy
{
    [Header("관통 옵션")]
    [SerializeField] private bool _ignoreBlock = false;
    [SerializeField] private bool _ignoreDodge = false;
    [SerializeField] private bool _ignoreGuard = false;
    [SerializeField] private bool _removeTargetStealth = false;

    [Header("시전자 은신 시 특공 보너스")]
    [SerializeField] private bool _empowerOnCasterStealth = false;

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        CEntity caster = context.Caster;
        CEntity target = context.Target;

        if (_removeTargetStealth && target.TokenManager.HasToken(EEffectType.Stealth))
        {
            target.TokenManager.RemoveTokenAll(EEffectType.Stealth);
            CPrint.Log($"[{effectData.Name}] : [{target.name}]의 [은신]을 간파하여 제거했습니다");
        }

        if (_ignoreGuard && target.TokenManager.HasToken(EEffectType.Guard))
        {
            target.TokenManager.RemoveTokenAll(EEffectType.Guard);
            target.SetGuardProtector(null);
            CPrint.Log($"[{effectData.Name}] : [{target.name}]의 [수호]를 무시하고 직접 타격합니다");
        }

        if (_ignoreDodge)
        {
            int removeDodge = target.TokenManager.RemoveTokenAll(EEffectType.Dodge) +
                              target.TokenManager.RemoveTokenAll(EEffectType.DodgePlus);
            if (removeDodge > 0)
            {
                CPrint.Log($"[{effectData.Name}] : [{target.name}]의 [회피] 토큰을 무시 합니다.");
            }
        }

        if (_ignoreBlock)
        {
            int removedBlock = target.TokenManager.RemoveTokenAll(EEffectType.Block) +
                               target.TokenManager.RemoveTokenAll(EEffectType.BlockPlus);
            if (removedBlock > 0)
            {
                CPrint.Log($"[{effectData.Name}] : [{target.name}]의 [방어] 토큰을 무시 합니다.");
            }
        }

        if (_empowerOnCasterStealth && caster.TokenManager.ConsumeToken(EEffectType.Stealth))
        {
            CPrint.Log($"[{caster.name}] [은신 기습] 은신 토큰을 소모하여 확정 치명타 및 공격력 증가가 적용됩니다.");
            caster.TokenManager.AddToken(EEffectType.Strength, 1);
            caster.TokenManager.AddToken(EEffectType.Crit, 1);
        }

        base.Apply(context, effectData);
    }
}
