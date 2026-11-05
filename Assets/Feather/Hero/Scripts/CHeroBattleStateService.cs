using System;
using System.Collections.Generic;

public class CHeroBattleStateService
{
    private readonly CHeroBattleSaveStore _store;
    private readonly CHeroMasteryService _mastery;
    private readonly Dictionary<CPlayer, string> _members = new Dictionary<CPlayer, string>();
    private CHeroBattleSaveData _data;
    private bool _isReadyForBattle;
    private bool _isRestoring;

    public string RunId
    {
        get { return _data != null ? _data.Heroes.RunId : string.Empty; }
    }
    public string BattleId
    {
        get { return _data != null ? _data.BattleId : string.Empty; }
    }
    public int CompletedBattles
    {
        get { return _data != null ? _data.Heroes.CompletedBattles : 0; }
    }
    public bool IsBattleFinished
    {
        get { return _data != null && _data.IsBattleFinished; }
    }
    public bool IsReadyForBattle
    {
        get { return _isReadyForBattle; }
    }
    public bool CanPrepareNextBattle
    {
        get
        {
            if (!IsBattleFinished) return false;
            for (int i = 0; i < _data.Heroes.Members.Length; i++)
                if (!_data.Heroes.Members[i].IsDead) return true;
            return false;
        }
    }

    public CHeroBattleStateService(string savePath, CHeroMasteryService mastery)
    {
        _store = new CHeroBattleSaveStore(savePath);
        _mastery = mastery;
    }

    public bool TryLoad(out string message)
    {
        message = "Finish restoring the current party first.";
        if (_isRestoring) return false;
        if (!_store.TryRead(out CHeroBattleSaveData next, out message)) return false;
        if (next != null && (_mastery == null || !_mastery.TryRestoreCheckpoint(next.Mastery)))
        {
            message = "Saved mastery is unavailable. Current state kept.";
            return false;
        }
        _data = next;
        _members.Clear();
        _isReadyForBattle = false;
        return true;
    }

    public bool TryStartExpedition(string runId, string battleId, IReadOnlyList<CPlayer> heroes,
        int masteryPoints, out string message)
    {
        message = "Expedition cannot start.";
        if (!CanUseHeroes(heroes) || string.IsNullOrWhiteSpace(runId) || runId.Length > 128
            || runId == RunId || masteryPoints < 0) return false;
        CHeroBattleSaveData next = new CHeroBattleSaveData
        {
            SaveId = Guid.NewGuid().ToString("N"),
            BattleId = battleId,
            Heroes = new CHeroRunSaveData { RunId = runId, Members = new CHeroRunMemberState[heroes.Count] },
            Mastery = new CHeroMasterySaveData { RunId = runId, Points = masteryPoints }
        };
        for (int i = 0; i < heroes.Count; i++)
        {
            if (!TryCaptureLoadout(heroes[i], out string[] skillIds, out message)) return false;
            next.Heroes.Members[i] = new CHeroRunMemberState
            {
                MemberId = Guid.NewGuid().ToString("N"), HeroId = heroes[i].HeroData.Id,
                Health = heroes[i].HeroData.MaxHealth,
                Rank = heroes[i].CurrentRank, SkillIds = skillIds
            };
        }
        return CommitAndRestore(next, heroes, out message);
    }

    public bool TryCompleteBattle(string runId, string battleId, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Battle is unavailable, already saved, or still acting.";
        if (_data == null || runId != RunId || battleId != BattleId || IsBattleFinished || !_isReadyForBattle
            || CompletedBattles == int.MaxValue || !CanUseHeroes(heroes)
            || heroes.Count != _data.Heroes.Members.Length) return false;
        CHeroMasterySaveData mastery = _mastery != null ? _mastery.CaptureCheckpoint() : null;
        if (mastery == null || mastery.RunId != RunId) return false;
        CHeroBattleSaveData next = Copy();
        next.SaveId = Guid.NewGuid().ToString("N");
        next.Mastery = mastery;
        next.IsBattleFinished = true;
        next.Heroes.CompletedBattles++;
        next.Heroes.LastBattleId = battleId;
        for (int i = 0; i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i];
            if (!_members.TryGetValue(hero, out string id) || id != _data.Heroes.Members[i].MemberId
                || hero.HeroData.Id != _data.Heroes.Members[i].HeroId
                || (hero.StressResolution != null && hero.StressResolution.IsPending)) return false;
            if (!TryCaptureLoadout(hero, out string[] skillIds, out message)) return false;
            CHeroRunMemberState member = hero.CaptureRunState(id);
            member.Rank = hero.CurrentRank;
            member.SkillIds = skillIds;
            next.Heroes.Members[i] = member;
        }
        if (!_store.TryWrite(next, _data.SaveId, out message)) return false;
        _data = next;
        _isReadyForBattle = false;
        return true;
    }

    public bool TryPrepareNextBattle(string runId, string nextBattleId, IReadOnlyList<CPlayer> heroes,
        out string message, bool useCurrentPartySetup = false)
    {
        message = "Finish the current battle with survivors before continuing.";
        if (runId != RunId || !CanPrepareNextBattle || nextBattleId == BattleId) return false;
        CHeroMasterySaveData mastery = _mastery != null ? _mastery.CaptureCheckpoint() : null;
        if (mastery == null || mastery.RunId != RunId) return false;
        CHeroBattleSaveData next = Copy();
        next.BattleId = nextBattleId;
        next.SaveId = Guid.NewGuid().ToString("N");
        next.IsBattleFinished = false;
        next.Mastery = mastery;
        if (useCurrentPartySetup)
        {
            message = "Party setup does not match the saved expedition.";
            if (!CanUseHeroes(heroes) || heroes.Count != next.Heroes.Members.Length) return false;
            for (int i = 0; i < heroes.Count; i++)
            {
                CPlayer hero = heroes[i];
                CHeroRunMemberState member = next.Heroes.Members[i];
                if (hero.HeroData.Id != member.HeroId
                    || (_members.TryGetValue(hero, out string memberId) && memberId != member.MemberId)) return false;
                if (!TryCaptureLoadout(hero, out string[] skillIds, out message)) return false;
                member.Rank = hero.CurrentRank;
                member.SkillIds = skillIds;
            }
        }
        return CommitAndRestore(next, heroes, out message);
    }

    public bool TryRestoreBattle(string runId, string battleId, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Saved battle does not match this party.";
        if (_data == null || runId != RunId || battleId != BattleId
            || !_store.IsCurrent(_data.SaveId) || !CanRestore(_data, heroes)) return false;
        if (!RestoreHeroes(_data, heroes)) return false;
        message = string.Empty;
        return true;
    }

    private bool CommitAndRestore(CHeroBattleSaveData next, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Battle state does not match this party.";
        if (!CanRestore(next, heroes)
            || !_store.TryWrite(next, _data != null ? _data.SaveId : string.Empty, out message)) return false;
        _data = next;
        return RestoreHeroes(next, heroes);
    }

    private bool CanRestore(CHeroBattleSaveData data, IReadOnlyList<CPlayer> heroes)
    {
        if (!CHeroBattleSaveStore.IsValid(data) || !CanUseHeroes(heroes) || _mastery == null
            || !_mastery.CanRestoreCheckpoint(data.Mastery) || heroes.Count != data.Heroes.Members.Length) return false;
        for (int i = 0; i < heroes.Count; i++)
        {
            CHeroRunMemberState member = data.Heroes.Members[i];
            if (!heroes[i].CanRestoreRunState(member)
                || !_mastery.CanApplyCheckpoint(heroes[i], data.Mastery,
                    member.Rank != ERank.None ? member.SkillIds : null)) return false;
        }
        return true;
    }

    private bool RestoreHeroes(CHeroBattleSaveData data, IReadOnlyList<CPlayer> heroes)
    {
        if (_isRestoring) return false;
        _isRestoring = true;
        try
        {
            _isReadyForBattle = false;
            if (!_mastery.TryRestoreCheckpoint(data.Mastery)) return false;
            _members.Clear();
            for (int i = 0; i < heroes.Count; i++)
            {
                CPlayer hero = heroes[i];
                CHeroRunMemberState member = data.Heroes.Members[i];
                if (member.Rank != ERank.None)
                {
                    if (!hero.TrySetRank(member.Rank) || !_mastery.TryApplyCheckpointLoadout(hero, member.SkillIds))
                        return false;
                    if (hero.RankPresentation != null)
                    {
                        hero.RankPresentation.SnapToRank();
                    }
                }
                if (!hero.TryRestoreRunState(member)) return false;
                hero.SkillUses.Clear();
                if (member.Rank == ERank.None && !hero.IsDead && !_mastery.TryApply(hero)) return false;
                _members.Add(hero, member.MemberId);
            }
            _isReadyForBattle = !data.IsBattleFinished;
            return true;
        }
        finally
        {
            _isRestoring = false;
        }
    }

    private bool TryCaptureLoadout(CPlayer hero, out string[] skillIds, out string message)
    {
        skillIds = null;
        message = "Hero rank or equipped skills cannot be saved.";
        return _mastery != null && CSkillRankUtility.IsSingleRank(hero.CurrentRank)
            && _mastery.TryCaptureSkillIds(hero, out skillIds);
    }

    private bool CanUseHeroes(IReadOnlyList<CPlayer> heroes)
    {
        if (_isRestoring || heroes == null || heroes.Count < 1 || heroes.Count > 4) return false;
        HashSet<CPlayer> seen = new HashSet<CPlayer>();
        for (int i = 0; i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i];
            if (hero == null || !seen.Add(hero) || hero.HeroData == null || hero.HeroData.MaxHealth <= 0
                || hero.TokenManager == null || hero.IsPresentationBusy
                || hero.ActionState == EEntityActionState.Requested
                || hero.ActionState == EEntityActionState.Executing) return false;
        }
        return true;
    }

    private CHeroBattleSaveData Copy()
    {
        CHeroRunMemberState[] members = new CHeroRunMemberState[_data.Heroes.Members.Length];
        for (int i = 0; i < members.Length; i++)
        {
            CHeroRunMemberState old = _data.Heroes.Members[i];
            members[i] = new CHeroRunMemberState
            {
                MemberId = old.MemberId, HeroId = old.HeroId, Health = old.Health,
                Stress = old.Stress, IsDead = old.IsDead, Rank = old.Rank,
                SkillIds = old.SkillIds != null ? (string[])old.SkillIds.Clone() : null
            };
        }
        return new CHeroBattleSaveData
        {
            SaveId = _data.SaveId,
            BattleId = _data.BattleId, IsBattleFinished = _data.IsBattleFinished,
            Heroes = new CHeroRunSaveData
            {
                RunId = RunId, CompletedBattles = CompletedBattles,
                LastBattleId = _data.Heroes.LastBattleId, Members = members
            },
            Mastery = new CHeroMasterySaveData
            {
                RunId = _data.Mastery.RunId, Points = _data.Mastery.Points,
                MasteredSkillIds = (string[])_data.Mastery.MasteredSkillIds.Clone()
            }
        };
    }
}
