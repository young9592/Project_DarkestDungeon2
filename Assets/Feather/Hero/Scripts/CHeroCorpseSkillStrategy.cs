using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Hero/Corpse Skill", fileName = "HeroCorpseSkillStrategy_")]
public class CHeroCorpseSkillStrategy : CEffectStrategy
{
    [Header("Corpse Clear")]
    [SerializeField] private bool _resetCooldowns;
    [Header("Dead of Night Rewards")]
    [SerializeField, Range(0, 100)] private int _healPercent;
    [SerializeField, Min(0)] private int _stealthTokens;
    [SerializeField, Min(0)] private int _stressRecovery;

    public bool ResetsCooldowns { get { return _resetCooldowns; } }
    public int HealPercent { get { return _healPercent; } }
    public int StealthTokens { get { return _stealthTokens; } }
    public int StressRecovery { get { return _stressRecovery; } }

    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !(context.Caster is CPlayer hero)
            || hero.IsDead || context.Target == null) return;

        CEnemyCorpse corpse = context.Target as CEnemyCorpse;
        if ((corpse != null && !corpse.IsAvailable) || (corpse == null && _healPercent > 0))
        {
            context.IsHit = false;
            return;
        }

        CheckAccuracy(context, effectData.BaseChance);
        if (corpse == null || !corpse.TryRemove()) return;

        // 제거는 명중 여부와 무관하며 실제 제거에 성공한 경우에만 보상합니다.
        if (_resetCooldowns) hero.SkillUses.ClearCooldowns();
        if (!context.IsHit || _healPercent <= 0) return;

        int heal = Mathf.RoundToInt(hero.HeroData.MaxHealth * (_healPercent / 100.0f));
        if (heal > 0) hero.TakeDamage(-heal);
        if (_stealthTokens > 0) hero.TokenManager.AddToken(EEffectType.Stealth, _stealthTokens);
        if (_stressRecovery > 0) hero.ModifyStress(-_stressRecovery);
    }

    private void CheckAccuracy(CEffectContext context, int criticalChance)
    {
        if (!context.IsHit) return;
        if (context.ConsumeCasterTokenOnce(EEffectType.Blind) && Random.Range(1, 101) <= 50)
        {
            context.IsHit = false;
            context.IsCrit = false;
            return;
        }
        if (context.Target.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            context.IsHit = Random.Range(1, 101) > 75;
        }
        else if (context.Target.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            context.IsHit = Random.Range(1, 101) > 50;
        }
        if (!context.IsHit)
        {
            context.IsCrit = false;
            return;
        }
        if (criticalChance > 0)
        {
            context.IsCrit = context.ConsumeCasterTokenOnce(EEffectType.Crit)
                || CHeroCriticalState.Roll(context.Caster, criticalChance);
        }
    }
}
