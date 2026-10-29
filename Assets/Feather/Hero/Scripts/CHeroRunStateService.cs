using System;
using System.Collections.Generic;

public class CHeroRunStateService
{
    private readonly CHeroRunSaveStore _store;
    private readonly Dictionary<CPlayer, string> _members = new Dictionary<CPlayer, string>();
    private CHeroRunSaveData _data;

    public bool HasRun { get { return _data != null; } }
    public string RunId { get { return _data != null ? _data.RunId : string.Empty; } }
    public int CompletedBattles { get { return _data != null ? _data.CompletedBattles : 0; } }
    public bool HasSurvivors
    {
        get
        {
            if (_data == null) return false;
            for (int i = 0; i < _data.Members.Length; i++)
            {
                if (!_data.Members[i].IsDead) return true;
            }
            return false;
        }
    }

    public CHeroRunStateService(string savePath)
    {
        _store = new CHeroRunSaveStore(savePath);
    }

    public bool TryLoad(out string message)
    {
        if (!_store.TryRead(out CHeroRunSaveData next, out message)) return false;
        _data = next;
        _members.Clear();
        return true;
    }

    public bool TryStartRun(string runId, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Finish the current action before starting an expedition.";
        if (!CanUseHeroes(heroes) || string.IsNullOrWhiteSpace(runId)
            || runId.Length > 128 || runId == RunId) return false;
        CHeroRunSaveData next = new CHeroRunSaveData
        {
            RunId = runId,
            Members = new CHeroRunMemberState[heroes.Count]
        };
        for (int i = 0; i < heroes.Count; i++)
        {
            next.Members[i] = new CHeroRunMemberState
            {
                MemberId = Guid.NewGuid().ToString("N"),
                HeroId = heroes[i].HeroData.Id,
                Health = heroes[i].HeroData.MaxHealth
            };
            if (!heroes[i].CanRestoreRunState(next.Members[i])) return false;
        }

        if (!_store.TryWrite(next, out message)) return false;
        _data = next;
        RestoreHeroes(heroes);
        return true;
    }

    public bool TrySaveBattle(string battleId, IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Expedition members are unavailable or still acting.";
        if (_data == null || !CanUseHeroes(heroes) || heroes.Count != _data.Members.Length
            || string.IsNullOrWhiteSpace(battleId) || battleId.Length > 128
            || _data.CompletedBattles == int.MaxValue) return false;
        if (_data.LastBattleId == battleId)
        {
            message = "This battle has already been saved.";
            return false;
        }

        CHeroRunSaveData next = new CHeroRunSaveData
        {
            RunId = _data.RunId,
            CompletedBattles = _data.CompletedBattles + 1,
            LastBattleId = battleId,
            Members = new CHeroRunMemberState[heroes.Count]
        };
        for (int i = 0; i < heroes.Count; i++)
        {
            if (!_members.TryGetValue(heroes[i], out string id)
                || id != _data.Members[i].MemberId || heroes[i].HeroData.Id != _data.Members[i].HeroId) return false;
            next.Members[i] = heroes[i].CaptureRunState(id);
        }

        if (!_store.TryWrite(next, out message)) return false;
        _data = next;
        return true;
    }

    public bool TryRestoreBattle(IReadOnlyList<CPlayer> heroes, out string message)
    {
        message = "Load the same expedition party before restoring its state.";
        if (_data == null || !CanUseHeroes(heroes) || heroes.Count != _data.Members.Length) return false;
        for (int i = 0; i < heroes.Count; i++)
        {
            if (!heroes[i].CanRestoreRunState(_data.Members[i])) return false;
        }

        RestoreHeroes(heroes);
        message = string.Empty;
        return true;
    }

    private bool CanUseHeroes(IReadOnlyList<CPlayer> heroes)
    {
        if (heroes == null || heroes.Count < 1 || heroes.Count > 4) return false;
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

    private void RestoreHeroes(IReadOnlyList<CPlayer> heroes)
    {
        _members.Clear();
        for (int i = 0; i < heroes.Count; i++)
        {
            heroes[i].TryRestoreRunState(_data.Members[i]);
            _members.Add(heroes[i], _data.Members[i].MemberId);
        }
    }
}
