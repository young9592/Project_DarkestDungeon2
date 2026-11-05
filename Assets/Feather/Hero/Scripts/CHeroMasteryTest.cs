using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroMasteryTest : MonoBehaviour
{
    private CHeroPartyTest _party;
    private CHeroBattleInput _input;
    private CHeroRunStateTest _runState;
    private CHeroMasteryCatalogSO _catalog;
    private CHeroMasteryService _service;
    private string _innId;
    private bool _inputWasEnabled;
    private Vector2 _scroll;
    private string _notice = "Open training to choose skill upgrades.";

    public CHeroMasteryService Service
    {
        get { return _service; }
    }
    public string Notice
    {
        get { return _notice; }
    }
    public string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "TempLee", "hero-mastery.json"); }
    }

    public void Initialize(CHeroMasteryCatalogSO catalog, CHeroBattleInput input)
    {
        if (_service != null) return;
        _party = GetComponent<CHeroPartyTest>();
        _runState = GetComponent<CHeroRunStateTest>();
        _input = input;
        _catalog = catalog;
        _service = new CHeroMasteryService(catalog, SavePath);
        if (!_service.TryLoad(out _notice)) return;
        _notice = "Mastery loaded. Points are shared by the party.";
    }

    public bool TryStartRun(string runId, int points)
    {
        if (!CanUseParty()) return false;
        if (string.IsNullOrWhiteSpace(runId) || runId == _service.RunId || points < 0)
        {
            _notice = "A new expedition ID and non-negative points are required.";
            return false;
        }
        CancelTraining();
        if (!_service.TryStartRun(runId, points, out _notice)) return false;
        return ApplyParty();
    }

    public bool TryAddPoints(int amount)
    {
        return _service != null && _service.TryAddPoints(amount, out _notice);
    }

    public bool TryOpenTraining()
    {
        if (!CanUseParty() || _service.IsTraining) return false;
        if (string.IsNullOrEmpty(_service.RunId))
        {
            _notice = "Start an expedition first.";
            return false;
        }
        _innId = Guid.NewGuid().ToString("N");
        if (!_service.TryBeginInnTraining(_innId, out _notice)) return false;
        if (_party.SelectedHero != null) _party.SelectedHero.CancelSkillSelection();
        _inputWasEnabled = _input != null && _input.enabled;
        if (_input != null) _input.enabled = false;
        _notice = "Training selections are not applied until confirmed.";
        return true;
    }

    public bool TryMaster(string skillId)
    {
        CHeroMasterySkill entry = _catalog != null ? _catalog.Find(skillId) : null;
        if (!CanUseParty() || entry == null || !HasHero(entry.Hero.Id)) return false;
        return _service.TryMaster(entry.Hero.Id, skillId, out _notice);
    }

    public bool TryResetTraining()
    {
        return _service != null && _service.TryResetTraining(out _notice);
    }

    public bool TryConfirmTraining()
    {
        if (!CanUseParty() || !_service.TryConfirmTraining(_innId, out _notice)) return false;
        RestoreInput();
        bool applied = ApplyParty();
        _notice = applied ? "Mastery saved and equipped." : "Mastery saved. Finish the action to equip it.";
        return applied;
    }

    public void CancelTraining()
    {
        if (_service == null || !_service.IsTraining) return;
        _service.CancelTraining();
        RestoreInput();
        _notice = "Training canceled. Points and skills kept.";
    }

    public bool TryPrepareHeroes(IReadOnlyList<CPlayer> heroes, string runId)
    {
        if (_catalog == null) return true;
        if (_service == null || heroes == null || _service.IsTraining) return false;
        if (!string.IsNullOrEmpty(runId) && runId != _service.RunId
            && !_service.TryStartRun(runId, 0, out _notice)) return false;
        if (string.IsNullOrEmpty(_service.RunId)) return true;
        for (int i = 0; i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i];
            if (hero == null || hero.IsDead || !hero.isActiveAndEnabled) continue;
            if (!_service.TryApply(hero)) return false;
        }
        return true;
    }

    public bool ApplyParty()
    {
        if (!CanUseParty()) return false;
        return TryPrepareHeroes(_party.Heroes, null);
    }

    private void Update()
    {
        if (_service == null || _party == null) return;
        if (_service.IsTraining && (!_party.IsOpen || _party.IsPreparing || _party.IsPreviewSuspended
            || !CTempLeeTestUI.CanShow(this))) CancelTraining();
        if (!_party.IsOpen || _party.IsBusy || _party.IsPreparing || _party.IsPreviewSuspended) return;
        if (_runState != null && _runState.IsExpeditionActive && _runState.Service.RunId != _service.RunId)
        {
            CancelTraining();
            if (!_service.TryStartRun(_runState.Service.RunId, 0, out _notice)) return;
        }
        if (!_service.IsTraining && !string.IsNullOrEmpty(_service.RunId)) ApplyParty();
    }

    private bool CanUseParty()
    {
        if (_service != null && _party != null && _party.IsOpen && !_party.IsBusy
            && !_party.IsPreparing && !_party.IsPreviewSuspended) return true;
        _notice = "Open the party and finish its current action first.";
        return false;
    }

    private bool HasHero(string heroId)
    {
        for (int i = 0; i < _party.Heroes.Count; i++)
        {
            CPlayer hero = _party.Heroes[i];
            if (hero != null && !hero.IsDead && hero.HeroData != null && hero.HeroData.Id == heroId) return true;
        }
        return false;
    }

    private void RestoreInput()
    {
        _innId = null;
        if (_input != null && _party != null && _party.IsOpen && !_party.IsPreparing
            && !_party.IsPreviewSuspended) _input.enabled = _inputWasEnabled;
    }

    private void OnDisable()
    {
        CancelTraining();
    }

    private void OnDestroy()
    {
        if (_service != null) _service.ReleaseRuntimeAssets();
    }

    private void OnGUI()
    {
        if (_service == null || _party == null || !_party.IsOpen || !_party.ShowControls
            || _party.IsPreparing || !CTempLeeTestUI.CanShow(this)) return;
        GUILayout.BeginArea(new Rect(Screen.width - 345, 345, 330, Mathf.Min(620, Screen.height - 360)), GUI.skin.box);
        GUILayout.Label("Skill Mastery / Points: " + _service.Points);
        bool previous = GUI.enabled;
        GUI.enabled = !_party.IsBusy && !_party.IsPreviewSuspended;
        if (!_service.IsTraining)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Train (test inn)")) TryOpenTraining();
            if (GUILayout.Button("Test reward +1")) TryAddPoints(1);
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirm")) TryConfirmTraining();
            if (GUILayout.Button("Reset selection")) TryResetTraining();
            if (GUILayout.Button("Cancel")) CancelTraining();
            GUILayout.EndHorizontal();
        }
        GUILayout.Label(_notice);
        _scroll = GUILayout.BeginScrollView(_scroll);
        for (int i = 0; i < _party.Heroes.Count; i++)
        {
            CPlayer hero = _party.Heroes[i];
            if (hero == null || hero.HeroData == null) continue;
            GUILayout.Label(hero.HeroData.Name + (hero.IsDead ? " / Dead" : ""));
            var skills = hero.HeroData.AvailableSkills;
            if (skills == null) continue;
            for (int j = 0; j < skills.Count; j++)
            {
                CHeroMasterySkill entry = _catalog != null ? _catalog.Find(hero.HeroData.Id, skills[j]) : null;
                if (entry == null) continue;
                bool mastered = _service.IsMastered(entry.Id) || _service.IsSelectedForMastery(entry.Id);
                GUI.enabled = !hero.IsDead && _service.CanMaster(hero.HeroData.Id, entry.Id, out _)
                    && !_party.IsBusy;
                string label = entry.BaseSkill.SkillName + (mastered ? " +" : entry.IsAvailable ? " / 1 point" : " / Unavailable");
                if (GUILayout.Button(new GUIContent(label, entry.RemainingEffects))) TryMaster(entry.Id);
            }
        }
        GUILayout.EndScrollView();
        GUI.enabled = previous;
        GUILayout.EndArea();
    }
}
