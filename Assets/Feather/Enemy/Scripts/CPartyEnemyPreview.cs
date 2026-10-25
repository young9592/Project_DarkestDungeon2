using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPartyEnemyPreview : MonoBehaviour
{
    [SerializeField] private CHeroPartyTest _partyTest;
    [SerializeField] private CEnemyGroupTest _groupTest;
    private readonly List<CEnemyAI> _actors = new List<CEnemyAI>();
    private readonly List<CEntity> _heroes = new List<CEntity>();
    private Coroutine _routine;
    private int _completed;
    private int _interrupted;
    private int _skipped;
    public IReadOnlyList<CEnemyAI> Actors
    {
        get { return _actors; }
    }

    public bool IsRunning
    {
        get { return _routine != null; }
    }

    public int Completed
    {
        get { return _completed; }
    }

    public int Interrupted
    {
        get { return _interrupted; }
    }

    public int Skipped
    {
        get { return _skipped; }
    }

    public bool HasPlan
    {
        get
        {
            for (int i = 0; i < _actors.Count; i++)
            {
                if (_actors[i] != null && _actors[i].PlannedSkill != null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void BindActors()
    {
        Detach();
        _completed = 0;
        _interrupted = 0;
        _skipped = 0;
        if (_groupTest == null || _partyTest == null)
        {
            return;
        }

        for (int i = 0; i < _groupTest.SpawnedEnemies.Count; i++)
        {
            CEnemy enemy = _groupTest.SpawnedEnemies[i];
            if (enemy == null)
            {
                continue;
            }

            CEnemyAI actor = enemy.GetComponent<CEnemyAI>();
            if (actor == null)
            {
                actor = enemy.gameObject.AddComponent<CEnemyAI>();
            }

            CEnemyIntentTest intent = enemy.GetComponent<CEnemyIntentTest>();
            if (intent != null)
            {
                intent.CancelIntent();
                intent.BindAutoAction(actor);
                intent.ShowLabel = false;
            }

            _actors.Add(actor);
            enemy.OnActionRequested += HandleRequest;
            enemy.OnActionCompleted += HandleCompleted;
            enemy.OnActionInterrupted += HandleInterrupted;
        }
    }

    public bool PlanAll()
    {
        if (!isActiveAndEnabled
            || _routine != null
            || _partyTest == null
            || !_partyTest.IsOpen
            || _partyTest.IsPreviewSuspended
            || _partyTest.IsPreparing
            || _partyTest.IsBusy
            || _actors.Count == 0)
        {
            return false;
        }

        _heroes.Clear();
        for (int i = 0; i < _partyTest.Heroes.Count; i++)
        {
            if (_partyTest.Heroes[i] != null)
            {
                _heroes.Add(_partyTest.Heroes[i]);
            }
        }

        bool planned = false;
        for (int i = 0; i < _actors.Count; i++)
        {
            if (_actors[i] != null && _actors[i].TryPlanAction(_heroes))
            {
                planned = true;
            }
        }

        return planned;
    }

    public bool RunPlannedActions()
    {
        if (!isActiveAndEnabled || _routine != null || !HasPlan || _partyTest == null || !_partyTest.TryBeginEnemyPreview())
        {
            return false;
        }

        _routine = StartCoroutine(RunActions());
        return true;
    }

    private IEnumerator RunActions()
    {
        // 현재 파티를 대상으로 적 연출 차례로 재생
        yield return null;
        for (int i = 0; i < _actors.Count; i++)
        {
            CEnemyAI actor = _actors[i];
            if (actor == null)
            {
                continue;
            }

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
            if ((!actor.IsPlanValid() && !actor.TryPlanAction(_heroes)) || !enemy.TryBeginAction())
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

            while (enemy != null && enemy.IsActionActive)
            {
                yield return null;
            }
        }

        _routine = null;
        _partyTest.EndEnemyPreview();
    }

    private void HandleRequest(CEntity actor, CEntity target, CSkillDataSO skill)
    {
        if (!actor.TryStartRequestedAction())
        {
            actor.CancelAction();
        }
    }

    private void HandleCompleted(CEntity actor)
    {
        _completed++;
    }

    private void HandleInterrupted(CEntity actor)
    {
        _interrupted++;
    }

    public void Detach()
    {
        bool wasRunning = _routine != null;
        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = null;
        for (int i = 0; i < _actors.Count; i++)
        {
            if (_actors[i] == null)
            {
                continue;
            }

            _actors[i].CancelPlan();
            CEnemy enemy = _actors[i].GetComponent<CEnemy>();
            enemy.OnActionRequested -= HandleRequest;
            enemy.OnActionCompleted -= HandleCompleted;
            enemy.OnActionInterrupted -= HandleInterrupted;
            CEnemyIntentTest intent = enemy.GetComponent<CEnemyIntentTest>();
            if (intent != null)
            {
                intent.BindAutoAction(null);
                intent.ShowLabel = true;
            }
        }

        _actors.Clear();
        _heroes.Clear();
        if (wasRunning && _partyTest != null)
        {
            _partyTest.EndEnemyPreview();
        }
    }

    private void OnDisable()
    {
        Detach();
    }
}
