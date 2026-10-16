using System;
using System.Collections.Generic;
using UnityEngine;

public class CHeroAnimation : MonoBehaviour
{
    private enum EAnimationState { Idle, Attack, Hit }

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private List<CSkillDataSO> _attackSkills = new List<CSkillDataSO>();

    private EAnimationState _state;
    private static readonly int _idleHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int _attackHash = Animator.StringToHash("Base Layer.AttackAntic");
    private static readonly int _attackEndHash = Animator.StringToHash("Base Layer.AttackRecover");
    private static readonly int _hitHash = Animator.StringToHash("Base Layer.Hit");
    private static readonly int _hitEndHash = Animator.StringToHash("Base Layer.HitRecover");

    public event Action OnAttackCompleted;
    public event Action OnAttackInterrupted;
    public event Action OnHitCompleted;
    public event Action OnHitInterrupted;

    public bool IsBusy { get { return _state != EAnimationState.Idle; } }
    public bool IsAttacking { get { return _state == EAnimationState.Attack; } }

    private void Awake()
    {
        if (_animator == null) _animator = GetComponentInChildren<Animator>(true);
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogError(name + ": Hero Animator와 Controller를 연결해야 합니다.", this);
            enabled = false;
        }
    }

    public bool TryPlayAttack(CSkillDataSO skill)
    {
        if (!CanPlay() || skill == null || _attackSkills == null || !_attackSkills.Contains(skill)) return false;
        if (!_animator.HasState(0, _attackHash) || !_animator.HasState(0, _attackEndHash)) return false;
        _state = EAnimationState.Attack;
        _animator.Play(_attackHash, 0, 0.0f);
        return true;
    }

    public bool TryPlayHit()
    {
        if (!CanPlay() || !_animator.HasState(0, _hitHash) || !_animator.HasState(0, _hitEndHash)) return false;
        _state = EAnimationState.Hit;
        _animator.Play(_hitHash, 0, 0.0f);
        return true;
    }

    private bool CanPlay()
    {
        return isActiveAndEnabled && !IsBusy && _animator != null && _animator.isActiveAndEnabled
            && _animator.runtimeAnimatorController != null && _animator.HasState(0, _idleHash);
    }

    private void LateUpdate()
    {
        if (!IsBusy) return;
        if (_animator == null || !_animator.isActiveAndEnabled || _animator.runtimeAnimatorController == null)
        {
            CancelPlayback();
            return;
        }
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        int endHash = IsAttacking ? _attackEndHash : _hitEndHash;
        if (info.fullPathHash != endHash || info.normalizedTime < 1.0f || _animator.IsInTransition(0)) return;
        EAnimationState finished = _state;
        PlayIdle();
        if (finished == EAnimationState.Attack) OnAttackCompleted?.Invoke();
        else OnHitCompleted?.Invoke();
    }

    public void CancelPlayback()
    {
        EAnimationState interrupted = _state;
        PlayIdle();
        if (interrupted == EAnimationState.Attack) OnAttackInterrupted?.Invoke();
        if (interrupted == EAnimationState.Hit) OnHitInterrupted?.Invoke();
    }

    private void PlayIdle()
    {
        _state = EAnimationState.Idle;
        if (_animator != null && _animator.isActiveAndEnabled && _animator.runtimeAnimatorController != null)
            _animator.Play(_idleHash, 0, 0.0f);
    }

    private void OnEnable()
    {
        PlayIdle();
    }

    private void OnDisable()
    {
        CancelPlayback();
    }
}
