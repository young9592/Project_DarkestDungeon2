using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Enemy/Pull", fileName = "EnemyPullStrategy_")]
public class CEnemyPullStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || effectData == null || !context.IsHit || context.Caster == null) return;

        CPlayer target = context.Target as CPlayer;
        CEnemyAI ai = context.Caster.GetComponent<CEnemyAI>();
        if (target == null || ai == null || !CEnemyTargetFilter.IsAvailable(target)
            || target.TokenManager == null || target.TokenManager.HasToken(EEffectType.Immobilize)
            || target.ActionState == EEntityActionState.Requested
            || target.ActionState == EEntityActionState.Executing) return;

        IReadOnlyList<CEntity> heroes = ai.HeroTargets;
        if (heroes == null || heroes.Count == 0 || heroes.Count > 4) return;

        int currentIndex = GetRankIndex(target.CurrentRank);
        if (currentIndex < 0 || effectData.Damage <= 0) return;
        int destinationIndex = Mathf.Max(0, currentIndex - effectData.Damage);
        if (destinationIndex == currentIndex) return;

        CPlayer[] members = new CPlayer[4];
        bool foundTarget = false;
        for (int i = 0; i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i] as CPlayer;
            if (hero == null || !CEnemyTargetFilter.IsAvailable(hero)) continue;
            int index = GetRankIndex(hero.CurrentRank);
            if (index < 0 || members[index] != null) return;
            members[index] = hero;
            if (hero == target) foundTarget = true;
        }
        if (!foundTarget) return;

        for (int i = currentIndex - 1; i >= destinationIndex; i--)
        {
            CPlayer displaced = members[i];
            if (displaced != null && (displaced.TokenManager == null
                || displaced.TokenManager.HasToken(EEffectType.Immobilize)
                || displaced.IsPresentationBusy
                || displaced.ActionState == EEntityActionState.Requested
                || displaced.ActionState == EEntityActionState.Executing)) return;
        }

        int resistance = target.EntityData != null ? target.EntityData.Resistance.Move : 0;
        int piercing = context.IsCrit ? 20 : 0;
        int chance = Mathf.Clamp(effectData.BaseChance + piercing - resistance, 0, 100);
        if (Random.Range(1, 101) > chance) return;

        target.TrySetRank((ERank)(1 << destinationIndex));
        for (int i = currentIndex - 1; i >= destinationIndex; i--)
        {
            CPlayer displaced = members[i];
            if (displaced != null) displaced.TrySetRank((ERank)(1 << (i + 1)));
        }
    }

    private int GetRankIndex(ERank rank)
    {
        for (int i = 0; i < 4; i++)
        {
            if (rank == (ERank)(1 << i)) return i;
        }
        return -1;
    }
}
