using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CHeroPartyMember
{
    [SerializeField] private CPlayer _prefab;
    [SerializeField] private ERank _rank;
    [SerializeField] private CHeroSkillLoadoutSO _loadout;
    public CPlayer Prefab
    {
        get { return _prefab; }
    }

    public ERank Rank
    {
        get { return _rank; }
    }

    public CHeroSkillLoadoutSO Loadout
    {
        get { return _loadout; }
    }

    public CHeroPartyMember(CPlayer prefab, ERank rank, CHeroSkillLoadoutSO loadout)
    {
        _prefab = prefab;
        _rank = rank;
        _loadout = loadout;
    }
}

[CreateAssetMenu(menuName = "Data/Hero/Party", fileName = "HeroPartySO_")]
public class CHeroPartySO : ScriptableObject
{
    [SerializeField] private List<CHeroPartyMember> _members = new List<CHeroPartyMember>();
    public IReadOnlyList<CHeroPartyMember> Members
    {
        get { return _members; }
    }

    public static CHeroPartySO CreateRuntime(IReadOnlyList<CHeroPartyMember> members)
    {
        if (members == null)
        {
            return null;
        }

        CHeroPartySO party = CreateInstance<CHeroPartySO>();
        party.hideFlags = HideFlags.DontSave;
        for (int i = 0; i < members.Count; i++)
        {
            party._members.Add(members[i]);
        }

        if (party.TryValidate(out _))
        {
            return party;
        }

        Destroy(party);
        return null;
    }

    public bool TryValidate(out string message)
    {
        if (_members == null || _members.Count != 4)
        {
            message = "파티에는 영웅 4명이 필요합니다.";
            return false;
        }

        ERank occupied = ERank.None;
        for (int i = 0; i < _members.Count; i++)
        {
            CHeroPartyMember member = _members[i];
            if (member == null
                || member.Prefab == null
                || member.Prefab.HeroData == null
                || !CSkillRankUtility.IsSingleRank(member.Rank)
                || (occupied & member.Rank) != ERank.None
                || member.Loadout == null
                || !member.Loadout.TryValidate(member.Prefab.HeroData, out message))
            {
                message = "영웅과 장착 목록, 배치 열의 중복 및 범위를 확인해야 합니다.";
                return false;
            }

            occupied |= member.Rank;
        }

        message = string.Empty;
        return true;
    }
}
