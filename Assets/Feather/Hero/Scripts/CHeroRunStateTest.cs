using System;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroRunStateTest : MonoBehaviour
{
    private CHeroPartyTest _party;
    private CHeroRunStateService _service;
    private string _battleId;
    private bool _battleSaved;
    private bool _isExpeditionActive;
    private CHeroPartySO _expeditionParty;
    private CHeroRunMemberState[] _closedPartyState;
    private string _notice = "Start a new expedition or continue a saved one.";

    public string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, "TempLee", "hero-run.json"); }
    }

    public string Notice { get { return _notice; } }
    public CHeroRunStateService Service { get { return _service; } }
    public bool IsExpeditionActive { get { return _isExpeditionActive; } }

    private void Awake()
    {
        _party = GetComponent<CHeroPartyTest>();
        _service = new CHeroRunStateService(SavePath);
        if (!_service.TryLoad(out string message)) _notice = message;
    }

    public bool TryStartExpedition()
    {
        if (!CanUseParty()) return false;
        if (!_service.TryStartRun(Guid.NewGuid().ToString("N"), _party.Heroes, out _notice)) return false;
        _isExpeditionActive = true;
        _expeditionParty = _party.ActiveParty;
        _closedPartyState = null;
        BeginBattle();
        SelectSurvivor();
        _notice = "New expedition: full HP, zero stress, all Heroes alive.";
        return true;
    }

    public bool TryContinueExpedition()
    {
        if (!CanUseParty() || !_service.TryLoad(out _notice)) return false;
        if (!_party.TryLoadParty(_party.ActiveParty, _service))
        {
            _notice = "Saved expedition does not match this party.";
            return false;
        }
        _isExpeditionActive = true;
        _expeditionParty = _party.ActiveParty;
        _closedPartyState = null;
        BeginBattle();
        _notice = _service.HasSurvivors ? "Saved Hero state restored." : "No survivors. Start a new expedition.";
        return true;
    }

    public bool TrySaveBattle()
    {
        if (!CanUseParty()) return false;
        if (string.IsNullOrEmpty(_battleId) || _battleSaved)
        {
            _notice = "Start an expedition or move to the next battle.";
            return false;
        }
        if (!_service.TrySaveBattle(_battleId, _party.Heroes, out _notice)) return false;
        _battleSaved = true;
        _notice = "Battle saved. Next battle will use these Hero states.";
        return true;
    }

    public bool TryNextBattle()
    {
        if (!CanUseParty() || (!_battleSaved && !TrySaveBattle())) return false;
        if (!_service.HasSurvivors)
        {
            _notice = "No survivors. Start a new expedition.";
            return false;
        }
        if (!_party.TryLoadParty(_party.ActiveParty, _service) || !_party.SwitchGroup(0))
        {
            _notice = "Could not prepare the next battle. Saved state kept.";
            return false;
        }
        BeginBattle();
        _notice = "Next battle: HP, stress and survival restored.";
        return true;
    }

    public void RememberParty()
    {
        if (!_isExpeditionActive || _party == null || !_party.IsOpen
            || _party.Heroes == null || _party.Heroes.Count == 0) return;

        CHeroRunMemberState[] states = new CHeroRunMemberState[_party.Heroes.Count];
        for (int i = 0; i < states.Length; i++)
        {
            if (_party.Heroes[i] == null) return;
            states[i] = _party.Heroes[i].CaptureRunState(_service.RunId + ":" + i);
            if (states[i] == null) return;
        }
        _expeditionParty = _party.ActiveParty;
        _closedPartyState = states;
    }

    public bool TryOpenParty(CHeroPartySO defaultParty)
    {
        if (!_isExpeditionActive) return _party.TryLoadParty(defaultParty, null);
        return TryRestoreParty(_expeditionParty);
    }

    public bool TryRestoreParty(CHeroPartySO party)
    {
        if (party == null || !party.TryValidate(out _)) return false;
        if (_closedPartyState != null)
        {
            if (_closedPartyState.Length != party.Members.Count) return false;
            for (int i = 0; i < _closedPartyState.Length; i++)
            {
                if (_closedPartyState[i].HeroId != party.Members[i].Prefab.HeroData.Id) return false;
            }
        }
        if (!_party.TryLoadParty(party, _service)) return false;
        _expeditionParty = party;
        if (_closedPartyState == null) return _party.RefreshHeroSelection();
        if (_closedPartyState.Length != _party.Heroes.Count) return false;
        for (int i = 0; i < _closedPartyState.Length; i++)
        {
            if (!_party.Heroes[i].CanRestoreRunState(_closedPartyState[i])) return false;
        }
        for (int i = 0; i < _closedPartyState.Length; i++)
        {
            if (!_party.Heroes[i].TryRestoreRunState(_closedPartyState[i])) return false;
        }
        return _party.RefreshHeroSelection();
    }

    private void BeginBattle()
    {
        _battleId = Guid.NewGuid().ToString("N");
        _battleSaved = false;
    }

    private bool CanUseParty()
    {
        if (_party != null && _party.IsOpen && !_party.IsPreparing
            && !_party.IsPreviewSuspended && !_party.IsBusy) return true;
        _notice = "Open the party and finish its current action first.";
        return false;
    }

    private void SelectSurvivor()
    {
        for (int i = 0; i < _party.Heroes.Count; i++)
        {
            if (!_party.Heroes[i].IsDead && _party.TrySelectHero(i)) return;
        }
    }

    private void OnGUI()
    {
        if (_party == null || !_party.IsOpen || !_party.ShowControls || _party.IsPreparing
            || !CTempLeeTestUI.CanShow(this)) return;
        GUILayout.BeginArea(new Rect(Screen.width - 345, 55, 330, 280), GUI.skin.box);
        GUILayout.Label("Hero expedition state / saved battles: " + _service.CompletedBattles);
        bool previous = GUI.enabled;
        GUI.enabled = !_party.IsBusy;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New Expedition")) TryStartExpedition();
        if (GUILayout.Button("Continue")) TryContinueExpedition();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Next Battle")) TryNextBattle();
        GUILayout.EndHorizontal();
        CPlayer hero = _party.SelectedHero;
        GUI.enabled = !_battleSaved && !_party.IsBusy && hero != null;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Damage 5")) hero.TakeDamage(5);
        if (GUILayout.Button("Stress +1")) hero.ModifyStress(1);
        GUILayout.EndHorizontal();
        GUI.enabled = previous;
        GUILayout.Label(_notice);
        for (int i = 0; i < _party.Heroes.Count; i++)
        {
            CPlayer member = _party.Heroes[i];
            GUILayout.Label(member.HeroData.Name + ": HP " + member.CurrentHp + " / Stress "
                + member.CurrentStress + (member.IsDead ? " / Dead" : member.IsDeathsDoor ? " / Death's Door" : " / Alive"));
        }
        GUILayout.EndArea();
    }
}
