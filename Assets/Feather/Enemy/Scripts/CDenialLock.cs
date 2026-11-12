using System.Collections.Generic;
using UnityEngine;

public enum EDenialRestriction
{
    None,
    Arms,
    Reach,
    Fortitude,
    Reason
}

[DisallowMultipleComponent]
[RequireComponent(typeof(CEnemy))]
public class CDenialLock : MonoBehaviour
{
    [Header("Denial")]
    [SerializeField] private EDenialRestriction _restriction;
    [SerializeField] private CSkillDataSO _uniqueSkill;
    [SerializeField] private CSkillDataSO _cripplingDoubt;
    [SerializeField] private CSkillDataSO _paralyzingFear;
    [Header("Despair Death Effect")]
    [SerializeField] private CEffectSO _blindOnHit;
    [SerializeField] private CEffectSO _weakOnHit;

    private CEffectSO _inheritedBlind;
    private CEffectSO _inheritedWeak;

    public EDenialRestriction Restriction
    {
        get { return _restriction; }
    }

    public CSkillDataSO UniqueSkill
    {
        get { return _uniqueSkill; }
    }

    public CSkillDataSO CripplingDoubt
    {
        get { return _cripplingDoubt; }
    }

    public CSkillDataSO ParalyzingFear
    {
        get { return _paralyzingFear; }
    }

    internal bool PreferUniqueAttack { get; set; }
    internal int DamageBonusPercent { get; private set; }
    internal int CriticalChanceBonus { get; private set; }

    internal void ApplyDeathEffect(IReadOnlyList<CEnemy> allies)
    {
        if (allies == null) return;

        for (int i = 0; i < allies.Count; i++)
        {
            CEnemy ally = allies[i];
            if (ally == null || !ally.isActiveAndEnabled || ally.IsDead || ally.IsDeathPresented
                || !ally.TryGetComponent(out CDenialLock survivor) || survivor == this) continue;

            switch (_restriction)
            {
                case EDenialRestriction.Arms:
                    survivor.DamageBonusPercent = 20;
                    break;
                case EDenialRestriction.Reach:
                    survivor.CriticalChanceBonus = 10;
                    break;
                case EDenialRestriction.Fortitude:
                    ally.TakeDamage(-Mathf.RoundToInt(ally.EnemyData.MaxHealth * 0.33f));
                    break;
                case EDenialRestriction.Reason:
                    survivor._inheritedBlind = _blindOnHit;
                    survivor._inheritedWeak = _weakOnHit;
                    break;
            }
        }
    }

    internal void ApplyInheritedOnHitEffect(CEffectContext context)
    {
        if (!context.IsHit || context.Target.IsDead || (_inheritedBlind == null && _inheritedWeak == null)) return;

        // 한 번 추첨해서 실명과 약화 중 하나만 시도함
        int roll = Random.Range(0, 100);
        CEffectSO effect = null;
        if (roll < 10) effect = _inheritedBlind;
        else if (roll < 20) effect = _inheritedWeak;
        if (effect != null && effect.Strategy != null) effect.Strategy.Apply(context, effect);
    }

    internal void ResetDeathEffects()
    {
        DamageBonusPercent = 0;
        CriticalChanceBonus = 0;
        _inheritedBlind = null;
        _inheritedWeak = null;
    }
}
