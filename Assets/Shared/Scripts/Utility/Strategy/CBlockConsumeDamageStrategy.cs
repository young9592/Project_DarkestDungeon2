using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/BlockConsumeDamageStrategy", fileName = "BlockConsumeDamageStrategy_")]
public class CBlockConsumeDamageStrategy : CDamageStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        bool consumedBlock = context.Caster.TokenManager.ConsumeToken(EEffectType.BlockPlus) ||
                             context.Caster.TokenManager.ConsumeToken(EEffectType.Block);

        if (consumedBlock)
        {
            CPrint.Log($"[{context.Caster.name}] : 자신의 [방어] 토큰을 소모하여 공격력을 증폭시킵니다");
            context.Caster.TokenManager.AddToken(EEffectType.Strength, 1);
        }

        base.Apply(context, effectData);
    }
}