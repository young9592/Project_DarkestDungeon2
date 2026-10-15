using UnityEngine;

public class CHeroBattleInputTest : MonoBehaviour
{
    [SerializeField] private CPlayer _hero;
    [SerializeField] private CHeroBattleInput _input;
    [SerializeField] private GameObject _viewRoot;
    [SerializeField] private CEnemyGroupTest _groupTest;
    [SerializeField] private CEnemyGroupSO[] _groups;
    [SerializeField] private MonoBehaviour[] _otherPanels;
    [SerializeField] private Vector3 _rankStep = new Vector3(0.01f, 0.0f, 0.0f);

    private bool _isOpen;
    private bool[] _panelStates;
    private int _receivedCount;
    private string _lastRequest = "No request yet";

    public int ReceivedCount { get { return _receivedCount; } }

    private void OnEnable()
    {
        if (_hero != null) _hero.OnActionRequested += HandleRequest;
    }

    private void OnDisable()
    {
        CloseTest();
        if (_hero != null) _hero.OnActionRequested -= HandleRequest;
    }

    public bool OpenTest()
    {
        if (!isActiveAndEnabled || _isOpen || _hero == null || _input == null || _viewRoot == null || _groupTest == null) return false;
        if (_otherPanels != null)
        {
            _panelStates = new bool[_otherPanels.Length];
            for (int i = 0; i < _otherPanels.Length; i++)
            {
                if (_otherPanels[i] == null) continue;
                _panelStates[i] = _otherPanels[i].enabled;
                _otherPanels[i].enabled = false;
            }
        }
        _hero.gameObject.SetActive(true);
        _viewRoot.SetActive(true);
        _groupTest.enabled = true;
        _groupTest.ShowControls = false;
        _isOpen = true;
        if (SwitchGroup(1)) return true;
        CloseTest();
        return false;
    }

    public bool SwitchGroup(int index)
    {
        if (!_isOpen || _groups == null || index < 0 || index >= _groups.Length) return false;
        if (!_groupTest.TryLoadGroup(_groups[index])) return false;
        _hero.CancelAction();
        _input.SetTargets(_groupTest.SpawnedEnemies, _rankStep);
        return _hero.TryBeginAction();
    }

    public void NextAction()
    {
        if (!_isOpen) return;
        if (_hero.ActionState == EEntityActionState.Executing)
        {
            if (!_hero.TryCompleteAction()) return;
        }
        else _hero.CancelAction();
        _hero.TryBeginAction();
    }

    private void HandleRequest(CEntity actor, CEntity target, CSkillDataSO skill)
    {
        if (!_isOpen) return;
        _receivedCount++;
        _lastRequest = skill.SkillName + " -> " + target.name;
        // 입력 테스트에서는 대상 피해 없이 Hero 공격 연출을 시작합니다.
        actor.TryStartRequestedAction();
    }

    public void CloseTest()
    {
        if (!_isOpen) return;
        _hero.CancelAction();
        _input.SetTargets(null, _rankStep);
        _viewRoot.SetActive(false);
        _hero.gameObject.SetActive(false);
        _groupTest.ClearGroup();
        _groupTest.enabled = false;
        if (_otherPanels != null)
            for (int i = 0; i < _otherPanels.Length; i++)
                if (_otherPanels[i] != null) _otherPanels[i].enabled = _panelStates[i];
        _isOpen = false;
    }

    private void OnGUI()
    {
        if (!_isOpen)
        {
            if (GUI.Button(new Rect(Screen.width - 150.0f, Screen.height - 90.0f, 140.0f, 25.0f), "Open Hero Input")) OpenTest();
            return;
        }
        GUILayout.BeginArea(new Rect(15.0f, 15.0f, 300.0f, 110.0f), GUI.skin.box);
        GUILayout.Label("Hero control / requests: " + _receivedCount);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1 slot")) SwitchGroup(0);
        if (GUILayout.Button("2 slots")) SwitchGroup(1);
        if (GUILayout.Button("3 slots")) SwitchGroup(2);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Next Action")) NextAction();
        if (GUILayout.Button("Close")) CloseTest();
        GUILayout.EndHorizontal();
        GUILayout.Label(_lastRequest);
        GUILayout.EndArea();
        if (GUI.Button(new Rect(Screen.width - 150.0f, Screen.height - 120.0f, 140.0f, 25.0f), "Hit Hero (1 damage)")) _hero.TakeDamage(1);
    }
}
