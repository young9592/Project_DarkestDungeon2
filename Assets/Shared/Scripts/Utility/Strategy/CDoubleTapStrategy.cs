using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/DoubleTapStrategy", fileName = "DoubleTapStrategy_")]
public class CDoubleTapStrategy : CDamageStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        CPrint.Log($"[{context.Caster.name}] <연속 사격> 1타 발사");
        base.Apply(context, effectData);

        if (!context.Target.IsDead)
        {
            CPrint.Log($"[{context.Caster.name}] <연속 사격> 2타 연속 발사");
            base.Apply(context, effectData);
        }
    }
}
