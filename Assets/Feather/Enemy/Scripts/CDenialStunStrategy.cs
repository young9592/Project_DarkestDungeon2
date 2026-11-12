public class CDenialStunStrategy : CApplyStatusStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit || context.Caster == null
            || context.Target == null || context.Target.IsDead
            || !context.Caster.TryGetComponent(out CEnemyAI ai)) return;

        int livingHeroes = 0;
        for (int i = 0; i < ai.HeroTargets.Count; i++)
        {
            if (CEnemyTargetFilter.IsAvailable(ai.HeroTargets[i])) livingHeroes++;
        }

        // 영웅이 두 명 이하일 때는 기절을 적용하지 않음
        if (livingHeroes > 2) base.Apply(context, effectData);
    }
}
