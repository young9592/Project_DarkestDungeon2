using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Dot Conversion", fileName = "HeroDotConversionStrategy_")]
public class CHeroDotConversionStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || !context.IsHit || effectData == null
            || !(context.Target is CEnemy enemy) || enemy.IsDead) return;

        long scaled = (long)enemy.ConsumeRemainingDotDamage() * Mathf.Max(0, effectData.Damage);
        int damage = (int)System.Math.Min(int.MaxValue, System.Math.Round(scaled / 100.0));
        if (damage > 0) enemy.TakeDamage(damage);
    }
}
