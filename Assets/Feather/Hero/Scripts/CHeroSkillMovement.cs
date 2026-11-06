using System.Collections.Generic;
using UnityEngine;

public class CHeroSkillMovement
{
    private IReadOnlyList<CPlayer> _party;
    private readonly List<CPlayer> _movedMembers = new List<CPlayer>();

    public void Configure(IReadOnlyList<CPlayer> party)
    {
        _party = party;
    }

    public bool TryMove(CPlayer hero, CSkillDataSO skill)
    {
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        if (hero == null || heroSkill == null || heroSkill.RankMovement == 0
            || !hero.isActiveAndEnabled || hero.IsDead || hero.IsDeathPresented
            || hero.ActionState != EEntityActionState.Executing
            || hero.TokenManager == null || hero.TokenManager.HasToken(EEffectType.Immobilize)
            || _party == null || _party.Count == 0 || _party.Count > 4
            || _movedMembers.Count > 0)
        {
            return false;
        }

        int previousIndex = GetRankIndex(hero.CurrentRank);
        if (previousIndex < 0) return false;
        int destinationIndex = Mathf.Clamp(previousIndex + heroSkill.RankMovement, 0, 3);
        if (destinationIndex == previousIndex) return false;

        CPlayer[] membersByRank = new CPlayer[4];
        bool foundHero = false;
        for (int i = 0; i < _party.Count; i++)
        {
            CPlayer member = _party[i];
            if (member == null) return false;
            int index = GetRankIndex(member.CurrentRank);
            if (index < 0 || membersByRank[index] != null) return false;
            membersByRank[index] = member;
            if (member == hero) foundHero = true;
        }

        if (!foundHero) return false;
        int direction = destinationIndex > previousIndex ? 1 : -1;
        for (int i = previousIndex + direction; i != destinationIndex + direction; i += direction)
        {
            CPlayer displaced = membersByRank[i];
            if (displaced != null && (displaced.TokenManager == null
                || displaced.TokenManager.HasToken(EEffectType.Immobilize)
                || displaced.IsPresentationBusy
                || displaced.ActionState == EEntityActionState.Requested
                || displaced.ActionState == EEntityActionState.Executing))
            {
                return false;
            }
        }

        _movedMembers.Add(hero);
        hero.TrySetRank((ERank)(1 << destinationIndex));
        for (int i = previousIndex + direction; i != destinationIndex + direction; i += direction)
        {
            CPlayer displaced = membersByRank[i];
            if (displaced != null)
            {
                _movedMembers.Add(displaced);
                displaced.TrySetRank((ERank)(1 << (i - direction)));
            }
        }
        return true;
    }

    public void CompleteMovement()
    {
        // 중단돼도 밀려난 아군까지 확정된 자리에 맞춤
        for (int i = 0; i < _movedMembers.Count; i++)
        {
            CPlayer member = _movedMembers[i];
            if (member != null && member.RankPresentation != null)
            {
                member.RankPresentation.SnapToRank();
            }
        }

        _movedMembers.Clear();
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
