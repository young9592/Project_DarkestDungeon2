using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyAutoActionTest : MonoBehaviour
{
    [Header("Temp_Lee AI Test")]
    [SerializeField] private CEnemyGroupTest _groupTest;
    [SerializeField] private CEnemyGroupSO _group;
    [SerializeField] private GameObject _heroRoot;
    [SerializeField] private CPlayer[] _heroes;
    [SerializeField] private CHeroActionTest _heroTest;

    private readonly List<CEnemyAI> _actors = new List<CEnemyAI>();
    private bool _isOpen;
    private bool _heroTestEnabled;
    private bool _groupTestEnabled;
    private bool _groupControlsVisible;
    private Coroutine _routine;
    private int _completed;
    private int _interrupted;
    private int _skipped;

    public IReadOnlyList<CEnemyAI> Actors { get { return _actors; } }

    public bool OpenTest()
    {
        if (!isActiveAndEnabled || _isOpen || _groupTest == null || _heroRoot == null) return false;
        if (_heroTest != null)
        {
            _heroTestEnabled = _heroTest.enabled;
            _heroTest.enabled = false;
        }
        _groupTestEnabled = _groupTest.enabled;
        _groupControlsVisible = _groupTest.ShowControls;
        _groupTest.enabled = true;
        if (!_groupTest.TryLoadGroup(_group))
        {
            _groupTest.enabled = _groupTestEnabled;
            if (_heroTest != null) _heroTest.enabled = _heroTestEnabled;
            return false;
        }
        _groupTest.ShowControls = false;
        _heroRoot.SetActive(true);
        _isOpen = true;
        for (int i = 0; i < _groupTest.SpawnedEnemies.Count; i++)
        {
            CEnemy enemy = _groupTest.SpawnedEnemies[i];
            CEnemyAI actor = enemy.GetComponent<CEnemyAI>();
            if (actor == null) actor = enemy.gameObject.AddComponent<CEnemyAI>();
            CEnemyIntentTest intent = enemy.GetComponent<CEnemyIntentTest>();
            if (intent != null) intent.BindAutoAction(actor);
            _actors.Add(actor);
            enemy.OnActionRequested += HandleRequest;
            enemy.OnActionCompleted += HandleCompleted;
            enemy.OnActionInterrupted += HandleInterrupted;
        }
        return true;
    }

    public void PlanAll()
    {
        if (!_isOpen || _routine != null) return;
        for (int i = 0; i < _actors.Count; i++)
            if (_actors[i] != null) _actors[i].TryPlanAction(_heroes, _groupTest.SpawnedEnemies);
    }

    public void RunPlannedActions()
    {
        if (!_isOpen || _routine != null) return;
        _routine = StartCoroutine(RunActions());
    }

    private IEnumerator RunActions()
    {
        // 타이머로 턴을 넘기지 않고 요청한 행동이 끝날 때까지 기다립니다.
        yield return null;
        for (int i = 0; i < _actors.Count; i++)
        {
            CEnemyAI actor = _actors[i];
            if (actor == null) continue;
            CEnemy enemy = actor.GetComponent<CEnemy>();
            while (enemy != null && enemy.isActiveAndEnabled && enemy.IsPresentationBusy)
            {
                yield return null;
            }

            bool canAct = enemy != null && enemy.OnTurnStart();
            while (enemy != null && enemy.isActiveAndEnabled && enemy.IsPresentationBusy)
            {
                yield return null;
            }

            if (!canAct || enemy == null || !enemy.isActiveAndEnabled)
            {
                _skipped++;
                actor.CancelPlan();
                continue;
            }

            // 턴 시작 피해로 의도가 해제되거나 HP 조건이 바뀌면 기존 AI로 다시 확인합니다.
            if ((!actor.IsPlanValid() && !actor.TryPlanAction(_heroes, _groupTest.SpawnedEnemies)) || !enemy.TryBeginAction())
            {
                _skipped++;
                actor.CancelPlan();
                continue;
            }
            if (!actor.TryRequestPlannedAction())
            {
                _skipped++;
                enemy.CancelAction();
                continue;
            }
            while (enemy != null && enemy.IsActionActive) yield return null;
        }
        _routine = null;
    }

    private void HandleRequest(CEntity actor, CEntity target, CSkillDataSO skill)
    {
        // 실제 전투 관리자 대신 연출 시작만 요청하고 효과는 적용하지 않습니다.
        actor.TryStartRequestedAction();
    }

    private void HandleCompleted(CEntity actor) { _completed++; }
    private void HandleInterrupted(CEntity actor) { _interrupted++; }

    public void CloseTest()
    {
        if (!_isOpen) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        for (int i = 0; i < _actors.Count; i++)
        {
            if (_actors[i] == null) continue;
            _actors[i].CancelPlan();
            CEnemy enemy = _actors[i].GetComponent<CEnemy>();
            enemy.OnActionRequested -= HandleRequest;
            enemy.OnActionCompleted -= HandleCompleted;
            enemy.OnActionInterrupted -= HandleInterrupted;
        }
        _actors.Clear();
        _heroRoot.SetActive(false);
        _groupTest.ClearGroup();
        _groupTest.ShowControls = _groupControlsVisible;
        _groupTest.enabled = _groupTestEnabled;
        if (_heroTest != null) _heroTest.enabled = _heroTestEnabled;
        _isOpen = false;
    }

    private void OnDisable() { CloseTest(); }

    private void OnGUI()
    {
        if (!CTempLeeTestUI.CanShow(this)) return;

        if (!_isOpen)
        {
            if (GUI.Button(new Rect(Screen.width - 150.0f, Screen.height - 60.0f, 140.0f, 25.0f), "Open Enemy AI")) OpenTest();
            return;
        }
        GUILayout.BeginArea(new Rect(15.0f, 15.0f, 300.0f, 105.0f), GUI.skin.box);
        GUILayout.Label("Enemy AI / choose skills and Hero targets");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Plan All")) PlanAll();
        if (GUILayout.Button("Run Actions")) RunPlannedActions();
        if (GUILayout.Button("Close")) CloseTest();
        GUILayout.EndHorizontal();
        GUILayout.Label("Done: " + _completed + " / Cancel: " + _interrupted + " / Skip: " + _skipped);
        GUILayout.Label(_routine != null ? "Playing planned actions" : "Plan first then run once");
        GUILayout.EndArea();
        Camera camera = Camera.main;
        if (camera == null || _heroes == null) return;
        for (int i = 0; i < _heroes.Length; i++)
        {
            if (_heroes[i] == null || !_heroes[i].isActiveAndEnabled) continue;
            Vector3 point = camera.WorldToScreenPoint(_heroes[i].transform.position);
            GUI.Box(new Rect(point.x - 45.0f, Screen.height - point.y - 25.0f, 90.0f, 22.0f), _heroes[i].name);
        }
    }
}
