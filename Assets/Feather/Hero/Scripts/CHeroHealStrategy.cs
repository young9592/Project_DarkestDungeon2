using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Heal", fileName = "HeroHealStrategy_")]
public class CHeroHealStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null || effectData == null)
        {
            return;
        }

        // 공격용 토큰을 소모하지 않고 회복 적용
        int healing = Mathf.Max(0, effectData.Damage);
        if (healing > 0)
        {
            context.Target.TakeDamage(-healing);
        }
    }
}
