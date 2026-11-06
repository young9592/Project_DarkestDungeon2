using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CHeroPreparationPanel : MonoBehaviour
{
    [Header("Party")]
    [SerializeField] private CHeroPartyTest _partyTest;
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private CHeroPartyPresets _presets;
    [Header("Selection")]
    [SerializeField] private Button[] _heroButtons;
    [SerializeField] private Button[] _skillButtons;
    [SerializeField] private Button[] _slotButtons;
    [SerializeField] private Button _moveLeft;
    [SerializeField] private Button _moveRight;
    [SerializeField] private Button _apply;
    [SerializeField] private Button _cancel;
    [SerializeField] private Text _heading;
    [SerializeField] private Text _detail;
    [SerializeField] private Text _status;
    private readonly List<CPlayer> _prefabs = new List<CPlayer>();
    private readonly List<ERank> _ranks = new List<ERank>();
    private readonly List<List<CSkillDataSO>> _skills = new List<List<CSkillDataSO>>();
    private readonly List<ScriptableObject> _runtimeAssets = new List<ScriptableObject>();
    [Header("Skill and Model Preview")]
    [SerializeField] private CHeroSkillDetailView _skillDetails;
    [SerializeField] private CHeroPreparationModel _modelPreview;
    private CSkillDataSO _inspectedSkill;
    private readonly List<CEntityHealthView> _hiddenHealthViews = new List<CEntityHealthView>();
    private readonly List<bool> _healthVisibility = new List<bool>();
    private int _selectedIndex;
    private bool _isOpen;
    public event System.Action<bool> OnClosed;
    public bool IsOpen
    {
        get { return _isOpen; }
    }

    public string SaveNotice
    {
        get { return _presets != null ? _presets.Notice : string.Empty; }
    }

    public int SelectedIndex
    {
        get { return _selectedIndex; }
    }

    public IReadOnlyList<ERank> Ranks
    {
        get { return _ranks; }
    }

    public IReadOnlyList<CSkillDataSO> SelectedSkills
    {
        get { return _skills.Count > _selectedIndex ? _skills[_selectedIndex] : null; }
    }

    private void Awake()
    {
        BindButtons(_heroButtons, SelectHero);
        BindButtons(_skillButtons, ToggleSkill);
        BindButtons(_slotButtons, RemoveSlot);
        if (_moveLeft != null)
        {
            _moveLeft.onClick.AddListener(() => MoveSelected(1));
        }

        if (_moveRight != null)
        {
            _moveRight.onClick.AddListener(() => MoveSelected(-1));
        }

        if (_apply != null)
        {
            _apply.onClick.AddListener(() => Apply());
        }

        if (_cancel != null)
        {
            _cancel.onClick.AddListener(Cancel);
        }

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private void BindButtons(Button[] buttons, System.Action<int> action)
    {
        if (buttons == null)
        {
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            if (buttons[i] != null)
            {
                buttons[i].onClick.AddListener(() => action(index));
            }
        }
    }

    public bool TryOpen()
    {
        if (_isOpen || _partyTest == null || _panelRoot == null || !_partyTest.BeginPreparation())
        {
            return false;
        }

        _prefabs.Clear();
        _ranks.Clear();
        _skills.Clear();
        for (int i = 0; i < _partyTest.Heroes.Count; i++)
        {
            CPlayer hero = _partyTest.Heroes[i];
            CPlayer prefab = FindPrefab(hero.HeroData);
            if (prefab == null)
            {
                _partyTest.EndPreparation();
                return false;
            }

            _prefabs.Add(prefab);
            _ranks.Add(hero.CurrentRank);
            List<CSkillDataSO> equipped = new List<CSkillDataSO>();
            for (int j = 0; j < hero.EquippedSkills.Count; j++)
            {
                equipped.Add(hero.EquippedSkills[j]);
            }

            _skills.Add(equipped);
        }

        _isOpen = true;
        HideHealthViews();
        _selectedIndex = 0;
        _inspectedSkill = _skills[0].Count > 0 ? _skills[0][0] : null;
        _panelRoot.SetActive(true);
        _panelRoot.transform.SetAsLastSibling();
        if (_detail != null)
        {
            _detail.text = "Hover to inspect / Click to equip or remove";
        }

        Refresh();
        if (_presets != null && !string.IsNullOrEmpty(_presets.Notice))
        {
            SetSaveNotice(_presets.Notice);
        }
        return true;
    }

    private CPlayer FindPrefab(CHeroDataSO data)
    {
        if (_partyTest.Parties == null)
        {
            return null;
        }

        for (int i = 0; i < _partyTest.Parties.Count; i++)
        {
            CHeroPartySO party = _partyTest.Parties[i];
            if (party == null || party.Members == null)
            {
                continue;
            }

            for (int j = 0; j < party.Members.Count; j++)
            {
                if (party.Members[j] != null && party.Members[j].Prefab != null && party.Members[j].Prefab.HeroData == data)
                {
                    return party.Members[j].Prefab;
                }
            }
        }

        return null;
    }

    public void SelectHero(int index)
    {
        if (!_isOpen || index < 0 || index >= _prefabs.Count)
        {
            return;
        }

        _selectedIndex = index;
        _inspectedSkill = _skills[index].Count > 0 ? _skills[index][0] : null;
        if (_detail != null)
        {
            _detail.text = "Hover to inspect / Click to equip or remove";
        }

        Refresh();
    }

    public bool MoveSelected(int direction)
    {
        if (!_isOpen || (direction != 1 && direction != -1))
        {
            return false;
        }

        int next = RankNumber(_ranks[_selectedIndex]) + direction;
        if (next < 1 || next > 4)
        {
            return false;
        }

        ERank rank = (ERank)(1 << (next - 1));
        for (int i = 0; i < _ranks.Count; i++)
        {
            if (_ranks[i] != rank)
            {
                continue;
            }

            _ranks[i] = _ranks[_selectedIndex];
            _ranks[_selectedIndex] = rank;
            Refresh();
            return true;
        }

        return false;
    }

    public void ToggleSkill(int index)
    {
        if (!_isOpen)
        {
            return;
        }

        IReadOnlyList<CSkillDataSO> available = _prefabs[_selectedIndex].HeroData.AvailableSkills;
        if (available == null || index < 0 || index >= available.Count || available[index] == null)
        {
            return;
        }

        CSkillDataSO skill = available[index];
        List<CSkillDataSO> equipped = _skills[_selectedIndex];
        ShowDetail(skill);
        if (equipped.Contains(skill))
        {
            equipped.Remove(skill);
        }
        else if (equipped.Count < 5)
        {
            equipped.Add(skill);
        }
        else
        {
            if (_status != null)
            {
                _status.text = "5 / 5 equipped. Remove a skill first.";
            }

            return;
        }

        Refresh();
    }

    public void RemoveSlot(int index)
    {
        if (!_isOpen || index < 0 || index >= _skills[_selectedIndex].Count)
        {
            return;
        }

        _inspectedSkill = _skills[_selectedIndex][index];
        _skills[_selectedIndex].RemoveAt(index);
        Refresh();
    }

    public bool Apply()
    {
        if (!_isOpen || !CanApply())
        {
            return false;
        }

        List<ScriptableObject> nextAssets = new List<ScriptableObject>();
        List<CHeroPartyMember> members = new List<CHeroPartyMember>();
        for (int i = 0; i < _prefabs.Count; i++)
        {
            CHeroSkillLoadoutSO loadout = CHeroSkillLoadoutSO.CreateRuntime(_prefabs[i].HeroData, _skills[i]);
            if (loadout == null)
            {
                Release(nextAssets);
                return false;
            }

            nextAssets.Add(loadout);
            members.Add(new CHeroPartyMember(_prefabs[i], _ranks[i], loadout));
        }

        CHeroPartySO party = CHeroPartySO.CreateRuntime(members);
        if (party == null)
        {
            Release(nextAssets);
            return false;
        }

        nextAssets.Add(party);
        // 적용한 편성은 런타임 복사본으로 보관
        _isOpen = false;
        if (!_partyTest.TryLoadParty(party))
        {
            _isOpen = true;
            Release(nextAssets);
            return false;
        }

        Release(_runtimeAssets);
        _runtimeAssets.AddRange(nextAssets);
        RestoreHealthViews();
        _panelRoot.SetActive(false);
        _partyTest.EndPreparation();
        _partyTest.TrySelectHero(_selectedIndex);
        if (_presets != null)
        {
            _presets.Close();
            if (TryCaptureParty(out CHeroSavedParty saved))
            {
                _presets.SaveApplied(saved);
            }
        }

        OnClosed?.Invoke(true);
        return true;
    }

    private bool CanApply()
    {
        if (_prefabs.Count != 4)
        {
            return false;
        }

        for (int i = 0; i < _skills.Count; i++)
        {
            if (_skills[i].Count < 1 || _skills[i].Count > 5)
            {
                return false;
            }
        }

        return true;
    }

    public void Cancel()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        if (_presets != null)
        {
            _presets.Close();
        }

        RestoreHealthViews();
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }

        if (_partyTest != null)
        {
            _partyTest.EndPreparation();
        }

        OnClosed?.Invoke(false);
    }

    private void Update()
    {
        if (!_isOpen)
        {
            return;
        }

        if (CTempLeeTestUI.CanShow(this) && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
            && _presets != null && _presets.HandleEscape())
        {
            return;
        }

        if (_partyTest == null || !_partyTest.IsOpen || (CTempLeeTestUI.CanShow(this) && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            Cancel();
        }
    }

    private void Refresh()
    {
        ShowDetail(_inspectedSkill);
        if (_modelPreview != null)
        {
            _modelPreview.Show(_prefabs[_selectedIndex], _partyTest.Heroes[_selectedIndex].transform.rotation);
        }

        if (_heading != null)
        {
            _heading.text = _prefabs[_selectedIndex].HeroData.Name + " / Rank " + RankNumber(_ranks[_selectedIndex]);
        }

        if (_heroButtons != null)
        {
            for (int i = 0; i < _heroButtons.Length && i < _prefabs.Count; i++)
            {
                Button button = _heroButtons[i];
                if (button == null)
                {
                    continue;
                }

                button.transform.SetSiblingIndex(4 - RankNumber(_ranks[i]));
                SetLabel(button, _prefabs[i].HeroData.Name + "\nRank " + RankNumber(_ranks[i]));
                button.image.color = i == _selectedIndex ? new Color(.48f, .32f, .12f) : new Color(.17f, .21f, .25f);
            }
        }

        IReadOnlyList<CSkillDataSO> available = _prefabs[_selectedIndex].HeroData.AvailableSkills;
        if (_skillButtons != null)
        {
            for (int i = 0; i < _skillButtons.Length; i++)
            {
                Button button = _skillButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool exists = available != null && i < available.Count && available[i] != null;
                button.gameObject.SetActive(exists);
                if (!exists)
                {
                    continue;
                }

                CSkillDataSO skill = available[i];
                int slot = _skills[_selectedIndex].IndexOf(skill);
                SetLabel(button, (slot >= 0 ? (slot + 1) + ". " : "") + skill.SkillName);
                button.image.color = slot >= 0 ? new Color(.4f, .31f, .13f) : new Color(.17f, .21f, .25f);
                Image icon = button.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = skill.SkillIcon;
                    icon.enabled = skill.SkillIcon != null;
                }
            }
        }

        if (_slotButtons != null)
        {
            for (int i = 0; i < _slotButtons.Length; i++)
            {
                if (_slotButtons[i] == null)
                {
                    continue;
                }

                bool equipped = i < _skills[_selectedIndex].Count;
                SetLabel(_slotButtons[i], (i + 1) + ". " + (equipped ? _skills[_selectedIndex][i].SkillName + "  x" : "Empty"));
                _slotButtons[i].interactable = equipped;
                Image icon = _slotButtons[i].transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = equipped ? _skills[_selectedIndex][i].SkillIcon : null;
                    icon.enabled = equipped && icon.sprite != null;
                }

            }
        }

        if (_moveLeft != null)
        {
            _moveLeft.interactable = RankNumber(_ranks[_selectedIndex]) < 4;
        }

        if (_moveRight != null)
        {
            _moveRight.interactable = RankNumber(_ranks[_selectedIndex]) > 1;
        }

        if (_apply != null)
        {
            _apply.interactable = CanApply();
        }

        if (_status != null)
        {
            _status.text = CanApply() ? _skills[_selectedIndex].Count
                + " / 5 equipped. Apply to enter the preview." : "Each Hero needs at least 1 equipped skill.";
        }
    }

    public void PreviewSkill(int index, bool equippedSlot)
    {
        if (!_isOpen) return;
        IReadOnlyList<CSkillDataSO> skills = equippedSlot ? _skills[_selectedIndex] : _prefabs[_selectedIndex].HeroData.AvailableSkills;
        if (skills == null || index < 0 || index >= skills.Count) return;
        ShowDetail(skills[index]);
    }

    public bool TryCaptureParty(out CHeroSavedParty saved)
    {
        saved = null;
        return _presets != null && CanApply() && _presets.TryCapture(_prefabs, _ranks, _skills, out saved);
    }

    public bool TryLoadSavedParty(CHeroSavedParty saved)
    {
        if (!_isOpen || _presets == null || !_presets.TryResolve(saved,
            out List<CPlayer> prefabs, out List<ERank> ranks, out List<List<CSkillDataSO>> skills))
        {
            return false;
        }

        // 전체 편성을 확인한 뒤 편집 중인 목록 교체
        _prefabs.Clear();
        _prefabs.AddRange(prefabs);
        _ranks.Clear();
        _ranks.AddRange(ranks);
        _skills.Clear();
        _skills.AddRange(skills);
        _selectedIndex = 0;
        _inspectedSkill = _skills[0][0];
        Refresh();
        return true;
    }

    public void SetSaveNotice(string message)
    {
        if (_status != null) _status.text = message;
    }

    private void HideHealthViews()
    {
        _hiddenHealthViews.Clear();
        _healthVisibility.Clear();
        CEntityHealthView[] views = FindObjectsOfType<CEntityHealthView>();
        for (int i = 0; i < views.Length; i++)
        {
            _hiddenHealthViews.Add(views[i]);
            _healthVisibility.Add(views[i].ShowBar);
            views[i].ShowBar = false;
        }
    }

    private void RestoreHealthViews()
    {
        for (int i = 0; i < _hiddenHealthViews.Count; i++)
        {
            if (_hiddenHealthViews[i] != null)
            {
                _hiddenHealthViews[i].ShowBar = _healthVisibility[i];
            }
        }

        _hiddenHealthViews.Clear();
        _healthVisibility.Clear();
    }

    private void ShowDetail(CSkillDataSO skill)
    {
        _inspectedSkill = skill;
        if (_skillDetails != null)
        {
            _skillDetails.Show(skill, _ranks[_selectedIndex], _skills[_selectedIndex].Contains(skill));
        }
    }

    private void SetLabel(Button button, string text)
    {
        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = text;
        }
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

    private string RankText(ERank ranks)
    {
        string text = "";
        for (int i = 0; i < 4; i++)
        {
            if (((int)ranks & (1 << i)) != 0)
            {
                text += (text.Length > 0 ? "," : "") + (i + 1);
            }
        }

        return text;
    }

    private void Release(List<ScriptableObject> assets)
    {
        for (int i = 0; i < assets.Count; i++)
        {
            if (assets[i] != null)
            {
                Destroy(assets[i]);
            }
        }

        assets.Clear();
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void OnDestroy()
    {
        Release(_runtimeAssets);
    }
}
