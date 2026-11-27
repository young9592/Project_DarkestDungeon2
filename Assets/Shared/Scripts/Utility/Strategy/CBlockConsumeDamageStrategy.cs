using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/BlockConsumeDamageStrategy", fileName = "BlockConsumeDamageStrategy_")]
public class CBlockConsumeDamageStrategy : CDamageStrategy
{
    [Header("조건부 처형 옵션")]
    [SerializeField, Min(0)] private int _executionBlockThreshold = 0;
    [SerializeField, Min(0)] private int _thresholdExecution = 0;

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null || effectData == null) return;

        int execution = effectData.Execution;

        int totalBlockCount = context.Caster.TokenManager.GetTokenStack(EEffectType.Block) +
                              context.Caster.TokenManager.GetTokenStack(EEffectType.BlockPlus);

        if (_executionBlockThreshold > 0 && _thresholdExecution > 0 && totalBlockCount >= _executionBlockThreshold)
        {
            execution = Mathf.Max(execution, _thresholdExecution);
            CPrint.Log($"[{context.Caster.name}] : [방어] 토큰 {totalBlockCount}개 보유 조건 충족으로 처형 {execution}이 적용됩니다.");
        }

        bool consumedBlock = context.Caster.TokenManager.ConsumeToken(EEffectType.BlockPlus) ||
                             context.Caster.TokenManager.ConsumeToken(EEffectType.Block);

        if (consumedBlock)
        {
            CPrint.Log($"[{context.Caster.name}] : 자신의 [방어] 토큰을 소모하여 공격력을 증폭시킵니다");
            context.Caster.TokenManager.AddToken(EEffectType.Strength, 1);
        }

        ApplyDamageInternal(context, effectData, execution);
    }
}