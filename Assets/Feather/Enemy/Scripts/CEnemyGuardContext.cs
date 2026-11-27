using System.Runtime.CompilerServices;

internal static class CEnemyGuardContext
{
    private static readonly ConditionalWeakTable<CEffectContext, CEffectContext> _guardContexts
        = new ConditionalWeakTable<CEffectContext, CEffectContext>();

    internal static void SetDefender(CEffectContext context, CEntity defender)
    {
        if (context == null || defender == null || context.Target == defender) return;
        if (_guardContexts.TryGetValue(context, out CEffectContext existing)) return;
        _guardContexts.Add(context, new CEffectContext(context.Caster, defender));
    }

    internal static CEffectContext GetEffectContext(CEffectContext context)
    {
        if (context == null || !_guardContexts.TryGetValue(context, out CEffectContext guarded)) return context;
        guarded.IsHit = context.IsHit;
        guarded.IsCrit = context.IsCrit;
        guarded.SkillCritChance = context.SkillCritChance;
        return guarded;
    }
}
