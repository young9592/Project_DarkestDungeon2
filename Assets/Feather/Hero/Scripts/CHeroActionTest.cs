using UnityEngine;

public class CHeroActionTest : MonoBehaviour
{
    [SerializeField] private CPlayer _hero;
    [SerializeField] private CEnemy[] _targets;
    [SerializeField] private MonoBehaviour[] _otherPanels;
    [SerializeField] private GameObject _testCharacter;
    [SerializeField] private CHeroSkillLoadoutSO[] _loadouts;

    private bool[] _panelStates;
    private bool _showControls;
    private int _targetIndex;
    private int _receivedCount;
    private string _message = "No request yet";
    private readonly string[] _rankLabels = { "1", "2", "3", "4" };

    private void OnEnable()
    {
        if (_hero != null) _hero.OnSkillActionRequested += HandleRequest;
    }

    private void OnDisable()
    {
        if (_hero != null) _hero.OnSkillActionRequested -= HandleRequest;
        SetTestVisible(false);
    }

    public void SetTestVisible(bool visible)
    {
        if (_showControls == visible) return;
        _showControls = visible;
        if (_testCharacter != null) _testCharacter.SetActive(visible);
        if (_hero != null) _hero.CancelSkillSelection();
        if (_otherPanels == null) return;
        if (visible)
        {
            // 조합 테스트에서 숨긴 원래 대상을 복구한 뒤 패널 상태를 저장합니다.
            for (int i = 0; i < _otherPanels.Length; i++)
            {
                CEnemyGroupTest groupTest = _otherPanels[i] as CEnemyGroupTest;
                if (groupTest != null) groupTest.ClearGroup();
            }
            _panelStates = new bool[_otherPanels.Length];
        }
        for (int i = 0; i < _otherPanels.Length; i++)
        {
            if (_otherPanels[i] == null) continue;
            if (visible) _panelStates[i] = _otherPanels[i].enabled;
            _otherPanels[i].enabled = visible ? false : _panelStates[i];
        }
    }

    public bool RequestSelectedTarget()
    {
        if (_hero == null || _targets == null || _targetIndex < 0 || _targetIndex >= _targets.Length) return false;
        CEnemy target = _targets[_targetIndex];
        bool sent = target != null && _hero.TryRequestSkillAction(target, target.CurrentRank);
        if (!sent) _message = "Request rejected / check skill and ranks";
        return sent;
    }

    private void HandleRequest(CPlayer actor, CEntity target, CSkillDataSO skill)
    {
        // 전달받은 행동 요청만 기록하고 피해는 적용하지 않습니다.
        _receivedCount++;
        _message = actor.name + " -> " + target.name + " / " + skill.SkillName;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 150.0f, Screen.height - 30.0f, 140.0f, 25.0f), _showControls ? "Close Hero Test" : "Open Hero Test"))
            SetTestVisible(!_showControls);
        if (!_showControls || _hero == null) return;
        GUILayout.BeginArea(new Rect(15.0f, 15.0f, 340.0f, 340.0f), GUI.skin.box);
        GUILayout.Label("Hero Action Test / request only");
        if (_loadouts != null)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _loadouts.Length; i++)
                if (_loadouts[i] != null && GUILayout.Button("Loadout " + (i + 1))) _hero.TryEquipLoadout(_loadouts[i]);
            GUILayout.EndHorizontal();
        }
        GUILayout.Label("Hero rank");
        int current = 0;
        for (int i = 0; i < 4; i++) if (_hero.CurrentRank == (ERank)(1 << i)) current = i;
        int rank = GUILayout.Toolbar(current, _rankLabels);
        if (rank != current) _hero.TrySetRank((ERank)(1 << rank));
        if (_hero.EquippedSkills != null)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _hero.EquippedSkills.Count; i++)
            {
                CSkillDataSO skill = _hero.EquippedSkills[i];
                if (skill != null && GUILayout.Button(skill.SkillName)) _hero.TrySelectSkill(i);
            }
            GUILayout.EndHorizontal();
            if (_targets != null && _targetIndex >= 0 && _targetIndex < _targets.Length && _targets[_targetIndex] != null)
            {
                CEnemy target = _targets[_targetIndex];
                int targetRank = 0;
                for (int i = 0; i < 4; i++) if (target.CurrentRank == (ERank)(1 << i)) targetRank = i;
                GUILayout.Label("Target rank");
                int selectedRank = GUILayout.Toolbar(targetRank, _rankLabels);
                if (selectedRank != targetRank) target.TrySetRank((ERank)(1 << selectedRank));
            }
        }
        GUILayout.Label("Selected: " + (_hero.SelectedSkill != null ? _hero.SelectedSkill.SkillName : "None"));
        if (_targets != null)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _targets.Length; i++)
            {
                CEnemy target = _targets[i];
                if (target == null) continue;
                bool valid = target.isActiveAndEnabled && CSkillRankUtility.CanTargetSkill(_hero.SelectedSkill, target.CurrentRank);
                string label = (_targetIndex == i ? "> " : "") + "Target " + (i + 1) + (valid ? " [OK]" : " [-]");
                if (GUILayout.Button(label)) _targetIndex = i;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Send Request")) RequestSelectedTarget();
        if (GUILayout.Button("Cancel")) _hero.CancelSkillSelection();
        GUILayout.EndHorizontal();
        GUILayout.Label("Requests: " + _receivedCount);
        GUILayout.Label(_message);
        GUILayout.EndArea();
    }
}
