using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CEncounterPreviewFlow : MonoBehaviour
{
    private enum EScreen
    {
        Closed,
        Preparing,
        EditingParty,
        Preview
    }

    [Header("Preview")]
    [SerializeField] private CHeroPartyTest _partyTest;
    [SerializeField] private CHeroPreparationPanel _preparation;
    [Header("Preparation Screen")]
    [SerializeField] private GameObject _selectionRoot;
    [SerializeField] private Button[] _groupButtons;
    [SerializeField] private Text _groupDetails;
    [SerializeField] private Text _partyDetails;
    [SerializeField] private Text _status;
    [SerializeField] private Button _editParty;
    [SerializeField] private Button _enter;
    [SerializeField] private Button _close;
    [Header("Preview Screen")]
    [SerializeField] private GameObject _previewRoot;
    [SerializeField] private Text _previewTitle;
    [SerializeField] private Button _return;
    [SerializeField] private CPartyEnemyPreview _enemyPreview;
    [SerializeField] private Button _planEnemies;
    [SerializeField] private Button _runEnemies;
    [SerializeField] private Text _enemyStatus;
    [SerializeField] private Text _intentSummary;
    private EScreen _screen;
    private int _groupIndex;
    private int _partyClosedFrame = -1;
    private bool _previousControls;
    private string _enemyMessage;
    public bool IsOpen
    {
        get { return _screen != EScreen.Closed; }
    }

    public bool IsPreviewing
    {
        get { return _screen == EScreen.Preview; }
    }

    public int GroupIndex
    {
        get { return _groupIndex; }
    }

    public CEnemyGroupSO SelectedGroup
    {
        get { return _partyTest != null
            && _partyTest.Groups != null
            && _groupIndex >= 0
            && _groupIndex < _partyTest.Groups.Count ? _partyTest.Groups[_groupIndex] : null; }
    }

    private void Awake()
    {
        if (_groupButtons != null)
        {
            for (int i = 0; i < _groupButtons.Length; i++)
            {
                int index = i;
                if (_groupButtons[i] != null)
                {
                    _groupButtons[i].onClick.AddListener(() => TryChooseGroup(index));
                }
            }
        }

        if (_editParty != null)
        {
            _editParty.onClick.AddListener(() => TryEditParty());
        }

        if (_enter != null)
        {
            _enter.onClick.AddListener(() => TryEnter());
        }

        if (_return != null)
        {
            _return.onClick.AddListener(() => TryReturn());
        }

        if (_close != null)
        {
            _close.onClick.AddListener(Close);
        }

        if (_planEnemies != null)
        {
            _planEnemies.onClick.AddListener(() =>
            {
                if (_enemyPreview != null)
                {
                    _enemyMessage = _enemyPreview.PlanAll() ? "Intent ready. Play enemy actions when ready." : "No enemy can act against this party.";
                }
            });
        }

        if (_runEnemies != null)
        {
            _runEnemies.onClick.AddListener(() =>
            {
                if (_enemyPreview != null)
                {
                    _enemyMessage = _enemyPreview.RunPlannedActions() ? "" : "Plan enemy actions before playing.";
                }
            });
        }

        SetScreens(false, false);
    }

    private void OnEnable()
    {
        if (_preparation != null)
        {
            _preparation.OnClosed += HandlePartyClosed;
        }
    }

    private void OnDisable()
    {
        if (_preparation != null)
        {
            _preparation.OnClosed -= HandlePartyClosed;
        }

        Close();
    }

    public bool TryOpen()
    {
        if (IsOpen
            || _partyTest == null
            || _preparation == null
            || _selectionRoot == null
            || _previewRoot == null
            || _partyTest.IsOpen
            || !_partyTest.OpenTest())
        {
            return false;
        }



        return OpenPreparation();
    }



    private bool OpenPreparation()
    {
        _previousControls = _partyTest.ShowControls;
        _partyTest.ShowControls = false;
        if (!_partyTest.SuspendPreview())
        {
            _partyTest.CloseTest();
            _partyTest.ShowControls = _previousControls;
            return false;
        }

        _groupIndex = 0;
        _screen = EScreen.Preparing;
        SetScreens(true, false);
        Refresh();
        if (_status != null && !string.IsNullOrEmpty(_preparation.SaveNotice))
        {
            _status.text = _preparation.SaveNotice;
        }

        return true;
    }

    public bool TryChooseGroup(int index)
    {
        if (_screen != EScreen.Preparing || _partyTest.Groups == null || index < 0 || index >= _partyTest.Groups.Count)
        {
            return false;
        }

        _groupIndex = index;
        Refresh();
        return true;
    }

    public bool TryEditParty()
    {
        if (_screen != EScreen.Preparing)
        {
            return false;
        }

        int validGroup = SelectedGroup != null && SelectedGroup.TryValidate(out _) ? _groupIndex : -1;
        if (validGroup < 0 && _partyTest.Groups != null)
        {
            for (int i = 0; i < _partyTest.Groups.Count; i++)
            {
                if (_partyTest.Groups[i] != null && _partyTest.Groups[i].TryValidate(out _))
                {
                    validGroup = i;
                    break;
                }
            }
        }

        if (validGroup < 0 || !_partyTest.ResumePreview(validGroup))
        {
            return false;
        }

        _screen = EScreen.EditingParty;
        SetScreens(false, false);
        if (_preparation.TryOpen())
        {
            return true;
        }

        _partyTest.SuspendPreview();
        _screen = EScreen.Preparing;
        SetScreens(true, false);
        Refresh();
        return false;
    }

    private void HandlePartyClosed(bool applied)
    {
        if (_screen != EScreen.EditingParty)
        {
            return;
        }

        _partyTest.SuspendPreview();
        _screen = EScreen.Preparing;
        _partyClosedFrame = Time.frameCount;
        SetScreens(true, false);
        Refresh();
        if (_status != null)
        {
            _status.text = applied ? "Party updated. Choose an encounter and enter." : "Changes discarded. Your previous party is ready.";
            if (applied && !string.IsNullOrEmpty(_preparation.SaveNotice))
            {
                _status.text = _preparation.SaveNotice;
            }
        }
    }

    public bool TryEnter()
    {
        if (_screen != EScreen.Preparing
            || SelectedGroup == null
            || !TryValidateEncounter(out _)
            || !_partyTest.ResumePreview(_groupIndex))
        {
            return false;
        }

        _screen = EScreen.Preview;
        _enemyMessage = "Plan enemy actions to reveal skills and targets.";
        if (_enemyPreview != null)
        {
            _enemyPreview.BindActors();
        }

        SetScreens(false, true);
        if (_previewTitle != null)
        {
            _previewTitle.text = GroupTitle(_groupIndex) + " / Select a Hero, skill and target";
        }

        return true;
    }

    public bool TryReturn()
    {
        if (_screen != EScreen.Preview || !_partyTest.SuspendPreview())
        {
            return false;
        }

        _screen = EScreen.Preparing;
        SetScreens(true, false);
        Refresh();
        return true;
    }

    public void Close()
    {


        if (!IsOpen)
        {
            return;
        }

        _screen = EScreen.Closed;
        SetScreens(false, false);
        if (_preparation != null && _preparation.IsOpen)
        {
            _preparation.Cancel();
        }

        if (_partyTest == null)
        {
            return;
        }

        _partyTest.CloseTest();
        _partyTest.ShowControls = _previousControls;
    }

    private void SetScreens(bool preparing, bool preview)
    {
        if (_selectionRoot != null)
        {
            _selectionRoot.SetActive(preparing);
        }

        if (_previewRoot != null)
        {
            _previewRoot.SetActive(preview);
        }
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (_partyTest == null || !_partyTest.IsOpen)
        {
            Close();
            return;
        }

        if (_return != null)
        {
            _return.interactable = !_partyTest.IsBusy;
        }

        if (_planEnemies != null)
        {
            _planEnemies.interactable = IsPreviewing && !_partyTest.IsBusy && _enemyPreview != null;
        }

        if (_runEnemies != null)
        {
            _runEnemies.interactable = IsPreviewing && !_partyTest.IsBusy && _enemyPreview != null && _enemyPreview.HasPlan;
        }

        if (_enemyStatus != null && _enemyPreview != null)
        {
            _enemyStatus.text = _enemyPreview.IsRunning ? "Playing enemy actions" : _enemyMessage + "\nCompleted: "
                + _enemyPreview.Completed
                + " / interrupted: "
                + _enemyPreview.Interrupted
                + " / skipped: "
                + _enemyPreview.Skipped;
        }

        if (_intentSummary != null && _enemyPreview != null)
        {
            _intentSummary.text = IntentSummary();
        }

        if (CTempLeeTestUI.CanShow(this)
            && _screen == EScreen.Preparing
            && _partyClosedFrame != Time.frameCount
            && Keyboard.current != null
            && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void Refresh()
    {
        if (_groupButtons != null)
        {
            for (int i = 0; i < _groupButtons.Length; i++)
            {
                if (_groupButtons[i] == null)
                {
                    continue;
                }

                bool available = _partyTest.Groups != null && i < _partyTest.Groups.Count;
                _groupButtons[i].gameObject.SetActive(available);
                if (!available)
                {
                    continue;
                }

                Text label = _groupButtons[i].GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = GroupTitle(i);
                }

                _groupButtons[i].image.color = i == _groupIndex ? new Color(.45f, .31f, .12f) : new Color(.17f, .21f, .25f);
            }
        }

        bool valid = TryValidateEncounter(out string message);
        if (_enter != null)
        {
            _enter.interactable = valid;
        }

        if (_groupDetails != null)
        {
            _groupDetails.text = GroupDetails();
        }

        if (_partyDetails != null)
        {
            _partyDetails.text = PartyDetails();
        }

        if (_status != null)
        {
            _status.text = valid ? "Ready. Enter with your current formation and equipped skills." : message;
        }
    }

    private string IntentSummary()
    {
        StringBuilder text = new StringBuilder();
        for (int i = 0; i < _enemyPreview.Actors.Count; i++)
        {
            CEnemyAI actor = _enemyPreview.Actors[i];
            if (actor == null || actor.PlannedSkill == null || actor.PlannedTarget == null)
            {
                continue;
            }

            CEnemyDataSO data = actor.GetComponent<CEnemy>().EnemyData;
            text.Append(actor.IsExecuting ? "Playing: " : "Next: ");
            text.Append(data.Name).Append(" / ").Append(SkillTitle(actor.PlannedSkill, data));
            text.Append(" > ").Append(actor.PlannedTarget.EntityData.Name);
            text.Append(" [").Append(RankNumber(actor.PlannedTarget.CurrentRank)).Append("]\n");
        }

        return text.Length > 0 ? text.ToString() : "Plan enemy actions to see skills and targets.";
    }

    private string GroupTitle(int index)
    {
        if (_partyTest.Groups == null || index < 0 || index >= _partyTest.Groups.Count || _partyTest.Groups[index] == null)
        {
            return "Missing group";
        }

        CEnemyGroupSO group = _partyTest.Groups[index];
        int count = group.Entries != null ? group.Entries.Count : 0;
        string title = string.IsNullOrEmpty(group.DisplayName) ? "Encounter " + (index + 1) : group.DisplayName;
        return title + " / " + count + " enemies";
    }

    private string GroupDetails()
    {
        if (SelectedGroup == null)
        {
            return "Choose an enemy group.";
        }

        if (!SelectedGroup.TryValidate(out _))
        {
            return "Invalid formation: missing enemy, overlapping ranks or beyond rank 4.";
        }

        StringBuilder text = new StringBuilder("FRONT  >  BACK\n");
        for (int rank = 1; rank <= 4; rank++)
        {
            string occupant = "Empty";
            for (int i = 0; i < SelectedGroup.Entries.Count; i++)
            {
                CEnemyGroupEntry entry = SelectedGroup.Entries[i];
                if (entry.TryGetOccupiedRanks(out ERank occupied) && (occupied & (ERank)(1 << (rank - 1))) != ERank.None)
                {
                    occupant = entry.EnemyPrefab.EnemyData.Name;
                    break;
                }
            }

            text.Append('[').Append(rank).Append("] ").Append(occupant);
            text.Append(rank == 4 ? "\n\n" : "    ");
        }

        for (int i = 0; i < SelectedGroup.Entries.Count; i++)
        {
            CEnemyGroupEntry entry = SelectedGroup.Entries[i];
            CEnemyDataSO data = entry.EnemyPrefab.EnemyData;
            text.Append(data.Name).Append(" / ").Append(data.FormationSize);
            text.Append(data.FormationSize > 1 ? " ranks\n" : " rank\n");
            if (data.AvailableSkills == null)
            {
                continue;
            }

            for (int j = 0; j < data.AvailableSkills.Count; j++)
            {
                CSkillDataSO skill = data.AvailableSkills[j];
                if (!CanPlanSkill(entry, skill))
                {
                    continue;
                }

                text.Append("  ").Append(SkillTitle(skill, data));
                text.Append(" > Hero rank ").Append(RankText(skill.TargetableRank)).Append('\n');
            }
        }

        return text.ToString();
    }

    private string SkillTitle(CSkillDataSO skill, CEnemyDataSO data)
    {
        string prefix = data.Name + " / ";
        string title = skill.SkillName;
        return title.StartsWith(prefix) ? title.Substring(prefix.Length) : title;
    }

    private bool TryValidateEncounter(out string message)
    {
        if (SelectedGroup == null || !SelectedGroup.TryValidate(out _))
        {
            message = "Choose a group without missing enemies or overlapping ranks.";
            return false;
        }

        CHeroPartySO party = _partyTest.ActiveParty;
        if (party == null || !party.TryValidate(out _))
        {
            message = "Set up a valid party and equipped skills first.";
            return false;
        }

        for (int i = 0; i < SelectedGroup.Entries.Count; i++)
        {
            CEnemyGroupEntry entry = SelectedGroup.Entries[i];
            CEnemyDataSO data = entry.EnemyPrefab.EnemyData;
            bool canAct = false;
            if (data.AvailableSkills != null)
            {
                for (int j = 0; j < data.AvailableSkills.Count; j++)
                {
                    if (CanPlanSkill(entry, data.AvailableSkills[j]))
                    {
                        canAct = true;
                        break;
                    }
                }
            }

            if (!canAct)
            {
                message = data.Name + " has no usable skill against this party. Choose another formation.";
                return false;
            }
        }

        message = "";
        return true;
    }

    private bool CanPlanSkill(CEnemyGroupEntry entry, CSkillDataSO skill)
    {
        if (!CSkillRankUtility.CanUseSkill(skill, entry.Rank))
        {
            return false;
        }

        CEnemyDataSO data = entry.EnemyPrefab.EnemyData;
        if (CEnemySkillSelector.GetSelectionWeight(data, skill, data.MaxHealth) <= 0)
        {
            return false;
        }

        CHeroPartySO party = _partyTest.ActiveParty;
        if (party == null || party.Members == null)
        {
            return false;
        }

        for (int i = 0; i < party.Members.Count; i++)
        {
            CHeroPartyMember member = party.Members[i];
            if (member != null && CSkillRankUtility.CanTargetSkill(skill, member.Rank))
            {
                return true;
            }
        }

        return false;
    }

    private string PartyDetails()
    {
        CHeroPartySO party = _partyTest.ActiveParty;
        if (party == null || party.Members == null)
        {
            return "No party selected.";
        }

        StringBuilder text = new StringBuilder();
        for (int rank = 4; rank >= 1; rank--)
        {
            for (int i = 0; i < party.Members.Count; i++)
            {
                CHeroPartyMember member = party.Members[i];
                if (member == null
                    || RankNumber(member.Rank) != rank
                    || member.Prefab == null
                    || member.Prefab.HeroData == null
                    || member.Loadout == null
                    || member.Loadout.Skills == null)
                {
                    continue;
                }

                text.Append('[').Append(rank).Append("] ").Append(member.Prefab.HeroData.Name).Append(" / ");
                for (int j = 0; j < member.Loadout.Skills.Count; j++)
                {
                    if (member.Loadout.Skills[j] == null)
                    {
                        continue;
                    }

                    if (j > 0)
                    {
                        text.Append(", ");
                    }

                    text.Append(member.Loadout.Skills[j].SkillName);
                }

                text.Append('\n');
            }
        }

        return text.ToString();
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

    private void OnGUI()
    {
        if (!CTempLeeTestUI.CanShow(this)) return;

        if (!IsOpen
            && _partyTest != null
            && !_partyTest.IsOpen
            && GUI.Button(new Rect(Screen.width - 180, Screen.height - 185, 170, 26), "Open Encounter"))
        {
            TryOpen();
        }
    }
}
