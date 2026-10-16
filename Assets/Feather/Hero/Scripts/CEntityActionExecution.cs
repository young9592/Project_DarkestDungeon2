using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CEntityActionExecution : MonoBehaviour
{
    [Header("Effect Execution")]
    [SerializeField, Min(1.0f)] private float _effectWaitLimit = 30.0f;

    private CEntity _actor;
    private Coroutine _routine;
    private readonly List<CEntity> _targets = new List<CEntity>();
    private bool _isRunning;
    private int _executionId;

    public bool IsRunning
    {
        get { return _isRunning; }
    }

    private void Awake()
    {
        _actor = GetComponent<CEntity>();
    }

    private void OnEnable()
    {
        if (_actor != null)
        {
            _actor.OnActionInterrupted += HandleInterrupted;
        }
    }

    public bool TryExecute(CSkillDataSO skill, IReadOnlyList<CEntity> targets, Func<bool> startAttack)
    {
        if (_isRunning || !isActiveAndEnabled || _actor == null
            || _actor.ActionState != EEntityActionState.Executing || skill == null
            || targets == null || targets.Count == 0 || startAttack == null)
        {
            return false;
        }

        bool hasEffects = skill.EffectList != null && skill.EffectList.Count > 0;
        CEffectManager effects = CEffectManager.Instance;
        if (hasEffects && (effects == null || !effects.isActiveAndEnabled))
        {
            return false;
        }

        for (int i = 0; hasEffects && i < skill.EffectList.Count; i++)
        {
            if (skill.EffectList[i] == null || skill.EffectList[i].Strategy == null)
            {
                return false;
            }
        }

        // 행동이 끝나기 전에 비워지는 대상 목록 복사
        _targets.Clear();
        for (int i = 0; i < targets.Count; i++)
        {
            CEntity target = targets[i];
            if (target == null || !target.isActiveAndEnabled || IsPresentationBusy(target))
            {
                _targets.Clear();
                return false;
            }

            if (!_targets.Contains(target))
            {
                _targets.Add(target);
            }
        }

        _isRunning = true;
        for (int i = 0; i < _targets.Count; i++)
        {
            BindHitInterruption(_targets[i], true);
        }

        int executionId = ++_executionId;
        if (!startAttack())
        {
            ResetExecution();
            return false;
        }

        if (!_isRunning || _actor.ActionState != EEntityActionState.Executing)
        {
            return false;
        }

        _routine = StartCoroutine(Execute(skill, effects, hasEffects, executionId));
        return true;
    }

    private IEnumerator Execute(CSkillDataSO skill, CEffectManager effects, bool hasEffects, int executionId)
    {
        yield return null;
        while (IsPresentationBusy(_actor))
        {
            if (!CanContinue(executionId))
            {
                Interrupt();
                yield break;
            }

            yield return null;
        }

        for (int i = 0; i < _targets.Count; i++)
        {
            if (!CanContinue(executionId))
            {
                Interrupt();
                yield break;
            }

            CEntity target = _targets[i];
            if (hasEffects)
            {
                if (effects == null || !effects.isActiveAndEnabled)
                {
                    Interrupt();
                    yield break;
                }

                bool effectsCompleted = false;
                float effectStartedAt = Time.realtimeSinceStartup;
                // 효과 완료 콜백과 실제 피격 종료를 따로 확인
                effects.ExecuteSkillEffects(_actor, target, skill, () =>
                {
                    if (_isRunning && executionId == _executionId)
                    {
                        effectsCompleted = true;
                    }
                });

                while (!effectsCompleted || IsPresentationBusy(target))
                {
                    if (!CanContinue(executionId) || effects == null || !effects.isActiveAndEnabled
                        || Time.realtimeSinceStartup - effectStartedAt >= _effectWaitLimit)
                    {
                        Interrupt();
                        yield break;
                    }

                    yield return null;
                }
            }
        }

        // 광역 대상과 자신에게 남은 연출까지 확인
        while (HasPendingPresentation())
        {
            if (!CanContinue(executionId))
            {
                Interrupt();
                yield break;
            }

            yield return null;
        }

        if (!CanContinue(executionId))
        {
            Interrupt();
            yield break;
        }

        _routine = null;
        ResetExecution();
        _actor.TryCompleteAction();
    }

    private bool CanContinue(int executionId)
    {
        if (!_isRunning || executionId != _executionId || _actor == null
            || !_actor.isActiveAndEnabled || _actor.ActionState != EEntityActionState.Executing)
        {
            return false;
        }

        for (int i = 0; i < _targets.Count; i++)
        {
            if (_targets[i] == null || !_targets[i].isActiveAndEnabled)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasPendingPresentation()
    {
        if (IsPresentationBusy(_actor))
        {
            return true;
        }

        for (int i = 0; i < _targets.Count; i++)
        {
            if (IsPresentationBusy(_targets[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPresentationBusy(CEntity entity)
    {
        CPlayer hero = entity as CPlayer;
        if (hero != null)
        {
            return hero.IsPresentationBusy;
        }

        CEnemy enemy = entity as CEnemy;
        return enemy != null && enemy.IsPresentationBusy;
    }

    private void Interrupt()
    {
        _routine = null;
        ResetExecution();
        if (_actor != null)
        {
            _actor.CancelAction();
        }
    }

    private void ResetExecution()
    {
        _isRunning = false;
        _executionId++;
        for (int i = 0; i < _targets.Count; i++)
        {
            BindHitInterruption(_targets[i], false);
        }

        _targets.Clear();
    }

    private void BindHitInterruption(CEntity target, bool bind)
    {
        CPlayer hero = target as CPlayer;
        CEnemy enemy = target as CEnemy;
        if (hero != null)
        {
            if (bind)
            {
                hero.OnHitPresentationInterrupted += HandleHitInterrupted;
            }
            else
            {
                hero.OnHitPresentationInterrupted -= HandleHitInterrupted;
            }
        }

        if (enemy != null)
        {
            if (bind)
            {
                enemy.OnHitPresentationInterrupted += HandleHitInterrupted;
            }
            else
            {
                enemy.OnHitPresentationInterrupted -= HandleHitInterrupted;
            }
        }
    }

    private void HandleHitInterrupted(CEntity target)
    {
        if (_actor != null)
        {
            _actor.CancelAction();
        }
    }

    private void HandleInterrupted(CEntity actor)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        ResetExecution();
    }

    private void OnDisable()
    {
        HandleInterrupted(_actor);
        if (_actor != null)
        {
            _actor.OnActionInterrupted -= HandleInterrupted;
            _actor.CancelAction();
        }
    }
}
