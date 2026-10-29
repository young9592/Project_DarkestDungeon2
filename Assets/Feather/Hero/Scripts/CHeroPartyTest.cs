using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CHeroPartyTest : MonoBehaviour
{
    [Header("Party")]
    [SerializeField] private CHeroPartySO[] _parties;
    [SerializeField] private Transform _partyRoot;
    [SerializeField] private Vector3 _frontPosition = new Vector3(0.014f, 0, 0);
    [SerializeField] private Vector3 _rankStep = new Vector3(0.011f, 0, 0);
    [Header("Preview")]
    [SerializeField] private CHeroBattleInput _input;
    [SerializeField] private GameObject _viewRoot;
    [SerializeField] private Button[] _heroButtons;
    [SerializeField] private CEnemyGroupTest _groupTest;
    [SerializeField] private CEnemyGroupSO[] _groups;
    [SerializeField] private MonoBehaviour[] _otherPanels;
    [SerializeField] private Camera _camera;
    [SerializeField] private CHeroPreparationPanel _preparation;
    [SerializeField] private CHeroPartyPresets _presets;
    [SerializeField] private CPartyEnemyPreview _enemyPreview;

    private readonly List<CPlayer> _heroes = new List<CPlayer>();
    private readonly List<ERank> _displayedRanks = new List<ERank>();
    private GameObject _loadedRoot;
    private bool _isOpen;
    private bool[] _panelStates;
    private Vector3 _cameraPosition;
    private float _cameraSize;
    private int _selectedIndex;
    private int _partyIndex;
    private int _requests;
    private Canvas _presentationCanvas;
    private GraphicRaycaster _presentationRaycaster;
    private string _lastRequest = "Choose a Hero, then a skill";
    private CHeroPartySO _activeParty;
    private bool _isPreviewSuspended;
    private bool _isEnemyPreview;
    public IReadOnlyList<CPlayer> Heroes
    {
        get { return _heroes; }
    }

    public IReadOnlyList<CHeroPartySO> Parties
    {
        get { return _parties; }
    }

    public IReadOnlyList<CEnemyGroupSO> Groups
    {
        get { return _groups; }
    }

    public CHeroPartySO ActiveParty
    {
        get { return _activeParty; }
    }

    public bool IsPreviewSuspended
    {
        get { return _isPreviewSuspended; }
    }

    public bool ShowControls { get; set; } = true;

    public bool IsOpen
    {
        get { return _isOpen; }
    }

    public bool IsPreparing
    {
        get { return _preparation != null && _preparation.IsOpen; }
    }

    public int ReceivedCount
    {
        get { return _requests; }
    }

    public CPlayer SelectedHero
    {
        get { return _input != null ? _input.Hero : null; }
    }

    public bool IsBusy
    {
        get
        {
            if (_isEnemyPreview)
            {
                return true;
            }

            for (int i = 0; i < _heroes.Count; i++)
            {
                if (_heroes[i] != null
                    && (_heroes[i].IsPresentationBusy
                    || _heroes[i].ActionState == EEntityActionState.Requested
                    || _heroes[i].ActionState == EEntityActionState.Executing))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool TryBeginEnemyPreview()
    {
        if (!_isOpen || _isPreviewSuspended || IsPreparing || IsBusy)
        {
            return false;
        }

        if (SelectedHero != null)
        {
            SelectedHero.CancelAction();
        }

        _isEnemyPreview = true;
        _input.enabled = false;
        return true;
    }

    public void EndEnemyPreview()
    {
        _isEnemyPreview = false;
        if (!_isOpen || _isPreviewSuspended || IsPreparing)
        {
            return;
        }

        _input.enabled = true;
        TrySelectHero(_selectedIndex);
    }

    private void Awake()
    {
        if (GetComponent<CHeroRunStateTest>() == null)
        {
            gameObject.AddComponent<CHeroRunStateTest>();
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_viewRoot != null)
        {
            _presentationCanvas = _viewRoot.GetComponentInChildren<Canvas>(true);
            _presentationRaycaster = _viewRoot.GetComponentInChildren<GraphicRaycaster>(true);
        }

        if (_heroButtons == null)
        {
            return;
        }

        for (int i = 0; i < _heroButtons.Length; i++)
        {
            int index = i;
            if (_heroButtons[i] != null)
            {
                _heroButtons[i].onClick.AddListener(() => TrySelectHero(index));
            }
        }
    }

    public bool BeginPreparation()
    {
        if (!_isOpen || _isPreviewSuspended || IsBusy || IsPreparing)
        {
            return false;
        }

        if (SelectedHero != null)
        {
            SelectedHero.CancelAction();
        }

        _input.enabled = false;
        if (_presentationCanvas != null)
        {
            _presentationCanvas.enabled = false;
        }

        if (_presentationRaycaster != null)
        {
            _presentationRaycaster.enabled = false;
        }

        return true;
    }

    public void EndPreparation()
    {
        if (!_isOpen)
        {
            return;
        }

        if (_presentationCanvas != null)
        {
            _presentationCanvas.enabled = true;
        }

        if (_presentationRaycaster != null)
        {
            _presentationRaycaster.enabled = true;
        }

        _input.enabled = true;
        TrySelectHero(_selectedIndex);
    }

    public bool OpenTest()
    {
        if (!isActiveAndEnabled) return false;


        if (_isOpen
            || _input == null
            || _viewRoot == null
            || _groupTest == null
            || _camera == null
            || _parties == null
            || _parties.Length == 0
            || _groups == null
            || _groups.Length == 0)
        {
            return false;
        }

        _partyIndex = 0;
        _lastRequest = "Choose a skill and target";
        _isOpen = true;
        if (_otherPanels != null)
        {
            _panelStates = new bool[_otherPanels.Length];
            for (int i = 0; i < _otherPanels.Length; i++)
            {
                if (_otherPanels[i] == null)
                {
                    continue;
                }

                _panelStates[i] = _otherPanels[i].enabled;
                _otherPanels[i].enabled = false;
            }
        }

        _cameraPosition = _camera.transform.position;
        _cameraSize = _camera.orthographicSize;
        _camera.transform.position += new Vector3(0.008f, 0, 0);
        _camera.orthographicSize = 0.026f;
        _viewRoot.SetActive(true);
        _groupTest.enabled = true;
        _groupTest.ShowControls = false;
        CHeroRunStateTest runState = GetComponent<CHeroRunStateTest>();
        bool activeExpedition = runState != null && runState.IsExpeditionActive;
        CHeroPartySO restored = !activeExpedition && _presets != null ? _presets.RestoreLastApplied() : null;
        CHeroPartySO openingParty = restored != null ? restored : _parties[0];
        bool loaded = runState != null ? runState.TryOpenParty(openingParty) : TryLoadParty(openingParty);
        if (loaded && SwitchGroup(0))
        {
            return true;
        }

        CloseTest();
        return false;
    }



    public bool TryLoadParty(CHeroPartySO party)
    {
        CHeroRunStateTest runState = GetComponent<CHeroRunStateTest>();
        if (runState != null && runState.IsExpeditionActive)
        {
            runState.RememberParty();
            return runState.TryRestoreParty(party);
        }
        return TryLoadParty(party, null);
    }

    public bool TryLoadParty(CHeroPartySO party, CHeroRunStateService runState)
    {
        if (!_isOpen || _isPreviewSuspended || IsBusy || _partyRoot == null || party == null || !party.TryValidate(out _))
        {
            return false;
        }

        GameObject nextRoot = new GameObject("Loaded Hero Party");
        nextRoot.transform.SetParent(_partyRoot, false);
        List<CPlayer> nextHeroes = new List<CPlayer>();
        for (int i = 0; i < party.Members.Count; i++)
        {
            CHeroPartyMember member = party.Members[i];
            CPlayer hero = Instantiate(member.Prefab, nextRoot.transform);
            hero.gameObject.SetActive(true);
            if (!hero.TrySetRank(member.Rank) || !hero.TryEquipLoadout(member.Loadout))
            {
                nextRoot.SetActive(false);
                Destroy(nextRoot);
                return false;
            }

            nextHeroes.Add(hero);
        }

        if (runState != null && !runState.TryRestoreBattle(nextHeroes, out _))
        {
            nextRoot.SetActive(false);
            Destroy(nextRoot);
            return false;
        }

        ClearParty();
        _lastRequest = "Choose a skill and target";
        _loadedRoot = nextRoot;
        _heroes.AddRange(nextHeroes);
        _activeParty = party;
        for (int i = 0; i < _heroes.Count; i++)
        {
            _heroes[i].OnActionRequested += HandleRequest;
        }

        UpdatePositions();
        UpdateButtonOrder();
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (!_heroes[i].IsDead && _heroes[i].isActiveAndEnabled)
            {
                return TrySelectHero(i);
            }
        }
        _selectedIndex = -1;
        _input.SetHero(null);
        return true;
    }

    public bool SuspendPreview()
    {
        if (!_isOpen || _isPreviewSuspended || IsPreparing || IsBusy)
        {
            return false;
        }

        if (_enemyPreview != null)
        {
            _enemyPreview.Detach();
        }

        CHeroRunStateTest runState = GetComponent<CHeroRunStateTest>();
        if (runState != null) runState.RememberParty();
        _isPreviewSuspended = true;
        ClearParty();
        _groupTest.SuspendGroup();
        _input.enabled = false;
        if (_presentationCanvas != null)
        {
            _presentationCanvas.enabled = false;
        }

        if (_presentationRaycaster != null)
        {
            _presentationRaycaster.enabled = false;
        }

        return true;
    }

    public bool ResumePreview(int groupIndex)
    {
        if (!_isOpen
            || !_isPreviewSuspended
            || _activeParty == null
            || !_activeParty.TryValidate(out _)
            || _groups == null
            || groupIndex < 0
            || groupIndex >= _groups.Length
            || _groups[groupIndex] == null
            || !_groups[groupIndex].TryValidate(out _))
        {
            return false;
        }

        _isPreviewSuspended = false;
        _input.enabled = true;
        if (_presentationCanvas != null)
        {
            _presentationCanvas.enabled = true;
        }

        if (_presentationRaycaster != null)
        {
            _presentationRaycaster.enabled = true;
        }

        if (!TryLoadParty(_activeParty) || !SwitchGroup(groupIndex))
        {
            SuspendPreview();
            return false;
        }

        _requests = 0;
        return true;
    }

    public bool TryMoveSelectedToNextRank()
    {
        if (!_isOpen || IsPreparing || SelectedHero == null || IsBusy)
        {
            return false;
        }

        int nextRank = RankNumber(SelectedHero.CurrentRank) % 4 + 1;
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (RankNumber(_heroes[i].CurrentRank) == nextRank)
            {
                return TrySwapRanks(_selectedIndex, i);
            }
        }

        return false;
    }

    private void UpdatePositions()
    {
        _displayedRanks.Clear();
        for (int i = 0; i < _heroes.Count; i++)
        {
            _heroes[i].RankPresentation.Configure(_frontPosition, _rankStep, _heroes);
            _displayedRanks.Add(_heroes[i].CurrentRank);
        }
    }

    private void UpdateButtonOrder()
    {
        if (_heroButtons == null)
        {
            return;
        }

        for (int rank = 4; rank >= 1; rank--)
        {
            for (int i = 0; i < _heroes.Count && i < _heroButtons.Length; i++)
            {
                if (_heroButtons[i] != null && RankNumber(_heroes[i].CurrentRank) == rank)
                {
                    _heroButtons[i].transform.SetSiblingIndex(4 - rank);
                }
            }
        }
    }

    public bool TrySelectHero(int index)
    {
        if (!_isOpen
            || _isPreviewSuspended
            || IsPreparing
            || IsBusy
            || index < 0
            || index >= _heroes.Count
            || _heroes[index].IsDeathPresented
            || !_heroes[index].isActiveAndEnabled)
        {
            return false;
        }

        if (SelectedHero != null)
        {
            SelectedHero.CancelAction();
        }

        _selectedIndex = index;
        if (!_input.SetHero(_heroes[index]))
        {
            return false;
        }

        BindTargets();
        return _heroes[index].TryBeginAction();
    }

    private void BindTargets()
    {
        _input.SetTargets(_groupTest.SpawnedEnemies, _rankStep);
        _input.AddAllyTargets(_heroes, _rankStep);
    }

    public bool SwitchGroup(int index)
    {
        if (!_isOpen
            || IsPreparing
            || IsBusy
            || _groups == null
            || index < 0
            || index >= _groups.Length
            || !_groupTest.TryLoadGroup(_groups[index]))
        {
            return false;
        }

        return RefreshHeroSelection();
    }

    public bool RefreshHeroSelection()
    {
        if (!_isOpen || _input == null || _heroes.Count == 0) return false;
        if (_selectedIndex >= 0 && _selectedIndex < _heroes.Count
            && !_heroes[_selectedIndex].IsDead && _heroes[_selectedIndex].isActiveAndEnabled)
        {
            return TrySelectHero(_selectedIndex);
        }
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (!_heroes[i].IsDead && _heroes[i].isActiveAndEnabled) return TrySelectHero(i);
        }
        _selectedIndex = -1;
        _input.SetHero(null);
        return true;
    }

    public bool TrySwapRanks(int first, int second)
    {
        if (!_isOpen
            || IsPreparing
            || IsBusy
            || first < 0
            || second < 0
            || first >= _heroes.Count
            || second >= _heroes.Count
            || first == second)
        {
            return false;
        }

        if (!TrySelectHero(_selectedIndex))
        {
            return false;
        }

        ERank previous = _heroes[first].CurrentRank;
        _heroes[first].TrySetRank(_heroes[second].CurrentRank);
        _heroes[second].TrySetRank(previous);
        UpdateButtonOrder();
        return true;
    }

    private void HandleRequest(CEntity actor, CEntity target, CSkillDataSO skill)
    {
        if (!_isOpen || actor != SelectedHero)
        {
            return;
        }

        _requests++;
        _lastRequest = "Last: "
            + HeroLabel((CPlayer)actor)
            + ": "
            + skill.SkillName
            + " -> "
            + ((CPlayer)actor).ActionTargets.Count
            + " target(s)";
        // 파티 미리보기에서 행동 연출 재생
        actor.TryStartRequestedAction();
    }

    private void Update()
    {


        if (!_isOpen || _heroButtons == null)
        {
            return;
        }

        for (int i = 0; i < _heroes.Count && i < _displayedRanks.Count; i++)
        {
            if (_heroes[i] != null && _displayedRanks[i] != _heroes[i].CurrentRank)
            {
                _displayedRanks[i] = _heroes[i].CurrentRank;
                UpdateButtonOrder();
            }
        }

        // 완료 후 같은 영웅 다시 선택
        if (!IsPreparing
            && !IsBusy
            && SelectedHero != null
            && !SelectedHero.IsDeathPresented
            && SelectedHero.isActiveAndEnabled
            && SelectedHero.ActionState == EEntityActionState.Idle)
        {
            SelectedHero.TryBeginAction();
        }

        for (int i = 0; i < _heroButtons.Length; i++)
        {
            if (_heroButtons[i] == null)
            {
                continue;
            }

            _heroButtons[i].interactable = !IsPreparing && !IsBusy && i < _heroes.Count
                && _heroes[i].isActiveAndEnabled && !_heroes[i].IsDeathPresented;
            _heroButtons[i].image.color = i == _selectedIndex ? new Color(0.65f, 0.42f, 0.12f) : new Color(0.18f, 0.23f, 0.28f);
            Text label = _heroButtons[i].GetComponentInChildren<Text>();
            if (label != null && i < _heroes.Count)
            {
                label.text = HeroLabel(_heroes[i]) + " / " + RankNumber(_heroes[i].CurrentRank);
            }
        }
    }

    private string HeroLabel(CPlayer hero)
    {
        return hero.EntityData.Id == "hero_man_at_arms" ? "ManAtArms" : hero.EntityData.Name;
    }

    private int RankNumber(ERank rank)
    {
        for (int i = 0; i < 4; i++)
        {
            if (rank == (ERank)(1 << i))
            {
                return i + 1;
            }
        }

        return 0;
    }

    private void ClearParty()
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i] == null)
            {
                continue;
            }

            _heroes[i].OnActionRequested -= HandleRequest;
            _heroes[i].CancelAction();
        }

        if (_input != null)
        {
            _input.SetHero(null);
        }
        _heroes.Clear();
        _displayedRanks.Clear();
        if (_loadedRoot != null)
        {
            _loadedRoot.SetActive(false);
            Destroy(_loadedRoot);
        }

        _loadedRoot = null;
    }

    public void CloseTest()
    {


        if (!_isOpen)
        {
            return;
        }

        CHeroRunStateTest runState = GetComponent<CHeroRunStateTest>();
        if (runState != null) runState.RememberParty();
        _isOpen = false;
        if (_enemyPreview != null)
        {
            _enemyPreview.Detach();
        }

        _isEnemyPreview = false;
        if (IsPreparing)
        {
            _preparation.Cancel();
        }

        _isPreviewSuspended = false;
        ClearParty();
        if (_groupTest != null)
        {
            _groupTest.ClearGroup();
            _groupTest.enabled = false;
        }

        if (_viewRoot != null)
        {
            _viewRoot.SetActive(false);
        }

        if (_input != null)
        {
            _input.enabled = true;
        }
        if (_presentationCanvas != null)
        {
            _presentationCanvas.enabled = true;
        }

        if (_presentationRaycaster != null)
        {
            _presentationRaycaster.enabled = true;
        }

        _activeParty = null;
        if (_camera != null)
        {
            _camera.transform.position = _cameraPosition;
            _camera.orthographicSize = _cameraSize;
        }

        if (_otherPanels != null && _panelStates != null)
        {
            for (int i = 0; i < _otherPanels.Length && i < _panelStates.Length; i++)
            {
                if (_otherPanels[i] != null)
                {
                    _otherPanels[i].enabled = _panelStates[i];
                }
            }
        }

    }

    private void OnDisable()
    {
        CloseTest();
    }

    private void OnGUI()
    {
        if (!CTempLeeTestUI.CanShow(this)) return;

        if (!ShowControls || IsPreparing)
        {
            return;
        }



        if (!_isOpen)
        {
            if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 150, 140, 25), "Open Hero Party"))
            {
                OpenTest();
            }

            return;
        }

        GUILayout.BeginArea(new Rect(15, 15, 340, 110), GUI.skin.box);
        GUILayout.Label("Hero party / requests: " + _requests);
        GUI.enabled = !IsPreparing && !IsBusy;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1 slot"))
        {
            SwitchGroup(0);
        }

        if (GUILayout.Button("2 slots"))
        {
            SwitchGroup(1);
        }

        if (GUILayout.Button("3 slots"))
        {
            SwitchGroup(2);
        }

        if (GUILayout.Button("Clear Skill"))
        {
            TrySelectHero(_selectedIndex);
        }

        if (GUILayout.Button("Close"))
        {
            CloseTest();
        }

        GUILayout.EndHorizontal();
        if (_isOpen)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Next Party"))
            {
                int next = (_partyIndex + 1) % _parties.Length;
                if (TryLoadParty(_parties[next]))
                {
                    _partyIndex = next;
                }
            }

            if (GUILayout.Button("Party / Skills") && _preparation != null)
            {
                _preparation.TryOpen();
            }

            if (GUILayout.Button("Move Next Rank"))
            {
                TryMoveSelectedToNextRank();
            }

            GUILayout.EndHorizontal();
            GUILayout.Label(_lastRequest);
        }

        GUILayout.EndArea();
        GUI.enabled = true;
    }
}
