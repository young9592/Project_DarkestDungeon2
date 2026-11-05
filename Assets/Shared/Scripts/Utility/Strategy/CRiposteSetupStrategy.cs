using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/RiposteSetupStrategy", fileName = "RiposteSetupStrategy_")]
public class CRiposteSetupStrategy : CEffectStrategy
{
    [Header("반격 발동 시 실행할 특수 반격 스킬")]
    [SerializeField] private CSkillDataSO _customRiposteSkill;

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null) return;

        int stackAmount = Mathf.Max(1, effectData.Damage);
        context.Target.TokenManager.AddToken(EEffectType.Riposte, stackAmount);

        if (_customRiposteSkill != null)
        {
            context.Target.SetRiposteSkill( _customRiposteSkill );
            CPrint.Log($"[특수 반격 장착] [{context.Target.name}] 반격 토큰 {stackAmount}개 흭득 (반격 스킬: <{_customRiposteSkill.SkillName}>");
        }
    }
}
