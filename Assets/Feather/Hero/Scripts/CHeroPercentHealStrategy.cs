using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Percent Heal", fileName = "HeroPercentHealStrategy_")]
public class CHeroPercentHealStrategy : CEffectStrategy
{
    [SerializeField, Range(1, 100)] private int _healthThreshold = 100;

    public int HealthThreshold
    {
        get { return _healthThreshold; }
    }

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || context.Target == null
            || context.Target.EntityData == null || context.Target.IsDead || effectData == null) return;

        if (_healthThreshold < 100 && context.Target is CPlayer hero
            && (long)hero.CurrentHp * 100 >= (long)hero.HeroData.MaxHealth * _healthThreshold) return;

        int percent = Mathf.Clamp(effectData.Damage, 0, 100);
        int amount = Mathf.RoundToInt(context.Target.EntityData.MaxHealth * (percent / 100f));
        if (amount > 0) context.Target.TakeDamage(-amount);
    }
}
