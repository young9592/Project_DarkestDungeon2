using UnityEngine;

public class CDenialRandomDotStrategy : CEffectStrategy
{
    [SerializeField] private CEffectSO[] _effects;

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || _effects == null || _effects.Length == 0) return;

        // 출혈, 중독, 화상 중 하나만 적용함
        CEffectSO selected = _effects[Random.Range(0, _effects.Length)];
        if (selected != null && selected.Strategy != null) selected.Strategy.Apply(context, selected);
    }
}
