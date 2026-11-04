using System.Collections.Generic;
using UnityEngine;

public class CHeroMasteryService
{
    private readonly CHeroMasteryCatalogSO _catalog;
    private readonly CHeroMasterySaveStore _store;
    private readonly Dictionary<CPlayer, CHeroSkillLoadoutSO> _loadouts = new Dictionary<CPlayer, CHeroSkillLoadoutSO>();
    private CHeroMasterySaveData _data;
    private bool _isLoaded;
    private CHeroMasterySaveData _pending;
    private string _innId;

    public bool IsTraining { get { return _pending != null; } }

    public int Points { get { return _pending != null ? _pending.Points : (_data != null ? _data.Points : 0); } }
    public string RunId { get { return _data != null ? _data.RunId : string.Empty; } }

    public CHeroMasteryService(CHeroMasteryCatalogSO catalog, string savePath)
    {
        _catalog = catalog;
        _store = new CHeroMasterySaveStore(savePath);
    }

    public bool TryLoad(out string message)
    {
        if (IsTraining)
        {
            message = "Finish or cancel mastery training before loading.";
            return false;
        }
        if (!_store.TryRead(out CHeroMasterySaveData next, out message)) return false;
        if (next == null)
        {
            _data = null;
            _isLoaded = true;
            return true;
        }
        if (!IsValid(next))
        {
            message = "Saved mastery skills are unavailable. Existing save kept.";
            return false;
        }
        _data = next;
        _isLoaded = true;
        return true;
    }

    public bool TryStartRun(string runId, int points, out string message)
    {
        message = string.Empty;
        if (IsTraining || !_isLoaded || string.IsNullOrWhiteSpace(runId) || points < 0)
        {
            message = "A run ID and non-negative points are required.";
            return false;
        }
        if (_data != null && _data.RunId == runId)
        {
            message = "This run has already started.";
            return false;
        }
        return Commit(new CHeroMasterySaveData { RunId = runId, Points = points }, out message);
    }

    public bool TryAddPoints(int amount, out string message)
    {
        message = string.Empty;
        if (IsTraining || !_isLoaded || _data == null || amount <= 0 || (long)_data.Points + amount > int.MaxValue)
        {
            message = "Mastery points cannot be added.";
            return false;
        }
        CHeroMasterySaveData next = Copy();
        next.Points += amount;
        return Commit(next, out message);
    }

    public bool IsMastered(string id)
    {
        if (!_isLoaded || _data == null || _data.MasteredSkillIds == null) return false;
        for (int i = 0; i < _data.MasteredSkillIds.Length; i++)
        {
            if (_data.MasteredSkillIds[i] == id) return true;
        }
        return false;
    }

    public bool CanMaster(string heroId, string skillId, out string message)
    {
        message = string.Empty;
        if (!_isLoaded || _data == null || _catalog == null)
        {
            message = "Mastery data is not ready.";
            return false;
        }
        if (!IsTraining)
        {
            message = "Skill mastery is available during inn training.";
            return false;
        }
        CHeroMasterySkill entry = _catalog.Find(skillId);
        if (entry == null || !entry.IsValid() || entry.Hero.Id != heroId)
        {
            message = "This skill cannot be mastered by this Hero.";
            return false;
        }
        if (!entry.IsAvailable)
        {
            message = "This skill mastery is not available yet.";
            return false;
        }
        if (IsSelectedForMastery(skillId))
        {
            message = "This skill is already mastered.";
            return false;
        }
        if (_pending.Points < entry.Cost)
        {
            message = "Not enough mastery points.";
            return false;
        }
        return true;
    }

    public bool TryMaster(string heroId, string skillId, out string message)
    {
        if (!CanMaster(heroId, skillId, out message)) return false;
        CHeroMasterySaveData next = Copy(_pending);
        next.Points -= _catalog.Find(skillId).Cost;
        List<string> ids = new List<string>(next.MasteredSkillIds);
        ids.Add(skillId);
        next.MasteredSkillIds = ids.ToArray();
        _pending = next;
        return true;
    }

    public bool TryBeginInnTraining(string innId, out string message)
    {
        message = string.Empty;
        if (!_isLoaded || _data == null || IsTraining || string.IsNullOrWhiteSpace(innId)
            || _catalog == null || !_catalog.TryValidate())
        {
            message = "Inn training cannot start.";
            return false;
        }
        _innId = innId;
        _pending = Copy();
        return true;
    }

    public bool TryResetTraining(out string message)
    {
        message = string.Empty;
        if (!IsTraining)
        {
            message = "Inn training is not open.";
            return false;
        }
        _pending = Copy();
        return true;
    }

    public bool TryConfirmTraining(string innId, out string message)
    {
        message = string.Empty;
        if (!IsTraining || _innId != innId)
        {
            message = "This inn training session is unavailable.";
            return false;
        }
        if (!Commit(_pending, out message)) return false;
        CancelTraining();
        return true;
    }

    public void CancelTraining()
    {
        _pending = null;
        _innId = null;
    }

    public bool IsSelectedForMastery(string skillId)
    {
        return IsTraining && System.Array.IndexOf(_pending.MasteredSkillIds, skillId) >= 0;
    }

    public CSkillDataSO ResolveTrainingSkill(string heroId, CSkillDataSO skill)
    {
        CHeroMasterySkill entry = _catalog != null ? _catalog.Find(heroId, skill) : null;
        if (entry != null && entry.IsValid() && entry.IsAvailable && IsSelectedForMastery(entry.Id)) return entry.MasteredSkill;
        return ResolveSkill(heroId, skill);
    }

    public CSkillDataSO ResolveSkill(string heroId, CSkillDataSO skill)
    {
        CHeroMasterySkill entry = _catalog != null ? _catalog.Find(heroId, skill) : null;
        if (entry == null || !entry.IsValid() || !entry.IsAvailable) return CHeroSkillSO.GetBaseSkill(skill);
        return IsMastered(entry.Id) ? entry.MasteredSkill : entry.BaseSkill;
    }

    public bool TryApply(CPlayer hero)
    {
        ReleaseUnusedLoadouts();
        if (!_isLoaded || _data == null || hero == null || hero.HeroData == null || hero.EquippedSkills == null
            || hero.IsPresentationBusy || hero.ActionState == EEntityActionState.Requested
            || hero.ActionState == EEntityActionState.Executing) return false;
        List<CSkillDataSO> skills = new List<CSkillDataSO>();
        bool changed = false;
        for (int i = 0; i < hero.EquippedSkills.Count; i++)
        {
            CSkillDataSO resolved = ResolveSkill(hero.HeroData.Id, hero.EquippedSkills[i]);
            skills.Add(resolved);
            if (resolved != hero.EquippedSkills[i]) changed = true;
        }
        if (!changed) return true;
        CHeroSkillLoadoutSO loadout = CHeroSkillLoadoutSO.CreateRuntime(hero.HeroData, skills);
        if (loadout == null) return false;
        if (!hero.TryEquipLoadout(loadout))
        {
            Object.Destroy(loadout);
            return false;
        }
        if (_loadouts.TryGetValue(hero, out CHeroSkillLoadoutSO previous)) Object.Destroy(previous);
        _loadouts[hero] = loadout;
        return true;
    }

    private void ReleaseUnusedLoadouts()
    {
        List<CPlayer> removed = new List<CPlayer>();
        foreach (KeyValuePair<CPlayer, CHeroSkillLoadoutSO> entry in _loadouts)
        {
            if (entry.Key != null) continue;
            Object.Destroy(entry.Value);
            removed.Add(entry.Key);
        }
        for (int i = 0; i < removed.Count; i++) _loadouts.Remove(removed[i]);
    }

    public void ReleaseRuntimeAssets()
    {
        foreach (CHeroSkillLoadoutSO loadout in _loadouts.Values) Object.Destroy(loadout);
        _loadouts.Clear();
    }

    private CHeroMasterySaveData Copy()
    {
        return Copy(_data);
    }

    private static CHeroMasterySaveData Copy(CHeroMasterySaveData data)
    {
        return new CHeroMasterySaveData { RunId = data.RunId, Points = data.Points,
            MasteredSkillIds = (string[])data.MasteredSkillIds.Clone() };
    }

    private bool IsValid(CHeroMasterySaveData data)
    {
        if (data == null || data.Version != 1 || string.IsNullOrWhiteSpace(data.RunId)
            || data.Points < 0 || data.MasteredSkillIds == null
            || _catalog == null || !_catalog.TryValidate()) return false;
        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < data.MasteredSkillIds.Length; i++)
        {
            string id = data.MasteredSkillIds[i];
            if (string.IsNullOrWhiteSpace(id) || _catalog.Find(id) == null
                || !_catalog.Find(id).IsAvailable || !ids.Add(id)) return false;
        }
        return true;
    }

    private bool Commit(CHeroMasterySaveData next, out string message)
    {
        message = string.Empty;
        if (!IsValid(next))
        {
            message = "Mastery data is not ready.";
            return false;
        }
        if (!_store.TryWrite(next, out message)) return false;
        _data = next;
        return true;
    }
}
