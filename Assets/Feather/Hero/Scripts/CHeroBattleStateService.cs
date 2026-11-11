using System;
using System.Collections.Generic;

public class CHeroBattleStateService
{
    private readonly CHeroBattleSaveStore _store;
    private readonly CHeroMasteryService _mastery;
    private readonly Dictionary<CPlayer, string> _members = new Dictionary<CPlayer, string>();
    private readonly Dictionary<string, CHeroRunMemberState> _fallenMembers = new Dictionary<string, CHeroRunMemberState>();
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

    public bool IsSavedMemberAlive(int index)
    {
        return _data != null && index >= 0 && index < _data.Heroes.Members.Length
            && !_data.Heroes.Members[index].IsDead;
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
        _fallenMembers.Clear();
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

    public bool TryRecordHeroDeath(string runId, string battleId, CPlayer hero, out string message)
    {
        message = "Hero death cannot be recorded for this battle.";
        if (!CanSaveBattle(runId, battleId) || hero == null || !hero.IsDead
            || !_members.TryGetValue(hero, out string memberId)) return false;
        if (_fallenMembers.ContainsKey(memberId))
        {
            message = string.Empty;
            return true;
        }
        if (!TryCaptureMember(hero, memberId, out CHeroRunMemberState member, out message)) return false;
        _fallenMembers.Add(memberId, member);
        message = string.Empty;
        return true;
    }

    public bool TryCompleteBattle(string runId, string battleId, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Battle is unavailable, already saved, or still acting.";
        if (!CanSaveBattle(runId, battleId) || CompletedBattles == int.MaxValue
            || heroes == null || heroes.Count > _data.Heroes.Members.Length) return false;
        var captured = new Dictionary<string, CHeroRunMemberState>(_fallenMembers);
        var seen = new HashSet<string>();
        for (int i = 0; i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i];
            // 삭제된 멤버는 사망 기록에서 채움
            if (hero == null) continue;
            if (!_members.TryGetValue(hero, out string id) || !seen.Add(id)) return false;
            if (hero.IsDead && captured.ContainsKey(id)) continue;
            if (!CanUseHero(hero) || (hero.StressResolution != null && hero.StressResolution.IsPending)
                || !TryCaptureMember(hero, id, out CHeroRunMemberState member, out message)) return false;
            captured[id] = member;
        }
        CHeroMasterySaveData mastery = _mastery != null ? _mastery.CaptureCheckpoint() : null;
        if (mastery == null || mastery.RunId != RunId) return false;
        CHeroBattleSaveData next = Copy();
        next.SaveId = Guid.NewGuid().ToString("N");
        next.Mastery = mastery;
        next.IsBattleFinished = true;
        next.Heroes.CompletedBattles++;
        next.Heroes.LastBattleId = battleId;
        for (int i = 0; i < next.Heroes.Members.Length; i++)
        {
            CHeroRunMemberState original = next.Heroes.Members[i];
            // 이전 전투에서 죽은 멤버는 새 전투에 생성하지 않아도 기록을 유지함
            if (original.IsDead && !captured.ContainsKey(original.MemberId)) continue;
            if (!captured.TryGetValue(original.MemberId, out CHeroRunMemberState member)
                || member.HeroId != original.HeroId)
            {
                message = "Battle member state is missing or does not match.";
                return false;
            }
            next.Heroes.Members[i] = member;
        }
        if (!_store.TryWrite(next, _data.SaveId, out message)) return false;
        _data = next;
        _fallenMembers.Clear();
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
            if (!TryMatchParty(next.Heroes.Members, heroes, out List<CHeroRunMemberState> members)) return false;
            for (int i = 0; i < heroes.Count; i++)
            {
                CPlayer hero = heroes[i];
                CHeroRunMemberState member = members[i];
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
            || !_store.IsCurrent(_data.SaveId)
            || !CanRestore(_data, heroes, out List<CHeroRunMemberState> members)) return false;
        if (!RestoreHeroes(_data, heroes, members)) return false;
        message = string.Empty;
        return true;
    }

    private bool CommitAndRestore(CHeroBattleSaveData next, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Battle state does not match this party.";
        if (!CanRestore(next, heroes, out List<CHeroRunMemberState> members)
            || !_store.TryWrite(next, _data != null ? _data.SaveId : string.Empty, out message)) return false;
        _data = next;
        return RestoreHeroes(next, heroes, members);
    }

    private bool TryMatchParty(CHeroRunMemberState[] saved, IReadOnlyList<CPlayer> heroes,
        out List<CHeroRunMemberState> members, bool checkBoundMembers = true)
    {
        members = new List<CHeroRunMemberState>();
        if (saved == null || !CanUseHeroes(heroes)) return false;
        bool includeDead = heroes.Count == saved.Length;
        for (int i = 0; i < saved.Length; i++)
        {
            if (includeDead || !saved[i].IsDead) members.Add(saved[i]);
        }
        if (heroes.Count != members.Count) return false;
        for (int i = 0; i < heroes.Count; i++)
        {
            if (heroes[i].HeroData.Id != members[i].HeroId
                || (checkBoundMembers && _members.TryGetValue(heroes[i], out string id)
                    && id != members[i].MemberId)) return false;
        }
        return true;
    }

    private bool CanRestore(CHeroBattleSaveData data, IReadOnlyList<CPlayer> heroes,
        out List<CHeroRunMemberState> members)
    {
        members = null;
        if (!CHeroBattleSaveStore.IsValid(data) || _mastery == null || !_mastery.CanRestoreCheckpoint(data.Mastery)
            || !TryMatchParty(data.Heroes.Members, heroes, out members, data.Heroes.RunId == RunId)) return false;
        for (int i = 0; i < heroes.Count; i++)
        {
            CHeroRunMemberState member = members[i];
            if (!heroes[i].CanRestoreRunState(member)
                || !_mastery.CanApplyCheckpoint(heroes[i], data.Mastery,
                    member.Rank != ERank.None ? member.SkillIds : null)) return false;
        }
        return true;
    }

    private bool RestoreHeroes(CHeroBattleSaveData data, IReadOnlyList<CPlayer> heroes,
        IReadOnlyList<CHeroRunMemberState> members)
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
                CHeroRunMemberState member = members[i];
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
            _fallenMembers.Clear();
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

    private bool CanSaveBattle(string runId, string battleId)
    {
        return !_isRestoring && _data != null && runId == RunId && battleId == BattleId
            && !IsBattleFinished && _isReadyForBattle;
    }

    private bool TryCaptureMember(CPlayer hero, string id, out CHeroRunMemberState member, out string message)
    {
        member = null;
        if (!TryCaptureLoadout(hero, out string[] skillIds, out message)) return false;
        member = hero.CaptureRunState(id);
        if (member == null) return false;
        member.Rank = hero.CurrentRank;
        member.SkillIds = skillIds;
        return true;
    }

    private bool CanUseHeroes(IReadOnlyList<CPlayer> heroes)
    {
        if (_isRestoring || heroes == null || heroes.Count < 1 || heroes.Count > 4) return false;
        HashSet<CPlayer> seen = new HashSet<CPlayer>();
        for (int i = 0; i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i];
            if (!CanUseHero(hero) || !seen.Add(hero)) return false;
        }
        return true;
    }

    private bool CanUseHero(CPlayer hero)
    {
        return hero != null && hero.HeroData != null && hero.HeroData.MaxHealth > 0
            && hero.TokenManager != null && !hero.IsPresentationBusy
            && hero.ActionState != EEntityActionState.Requested
            && hero.ActionState != EEntityActionState.Executing;
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
