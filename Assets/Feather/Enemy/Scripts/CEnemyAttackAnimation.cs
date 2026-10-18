using System;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyAttackAnimation : MonoBehaviour
{
    private enum EAnimationState
    {
        Idle,
        Attack,
        Hit
    }

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [Header("Skill Animations")]
    [SerializeField] private List<CEnemySkillAnimation> _skillAnimations = new List<CEnemySkillAnimation>();
    private static readonly int _idleStateHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int _attackStateHash = Animator.StringToHash("Base Layer.Attack");
    private static readonly int _attackPoseStateHash = Animator.StringToHash("Base Layer.AttackPose");
    private static readonly int _hitStateHash = Animator.StringToHash("Base Layer.Hit");
    private static readonly int _hitRecoverStateHash = Animator.StringToHash("Base Layer.HitRecover");
    private bool _isReady;
    private EAnimationState _state;
    private bool _needsIdle;
    private int _currentAttackCompletionHash;
    // 비활성화로 중단되면 완료 알림 생략
    public event Action OnAttackStarted;
    public event Action OnAttackCompleted;
    public event Action OnHitStarted;
    public event Action OnHitCompleted;
    public event Action OnAttackInterrupted;
    public event Action OnHitInterrupted;
    public bool IsAttacking
    {
        get { return _state == EAnimationState.Attack; }
    }

    public bool IsHitReacting
    {
        get { return _state == EAnimationState.Hit; }
    }

    public bool IsBusy
    {
        get { return _state != EAnimationState.Idle; }
    }

    public bool IsReady
    {
        get { return _isReady
            && isActiveAndEnabled
            && _animator != null
            && _animator.isActiveAndEnabled
            && _animator.runtimeAnimatorController != null; }
    }

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"{name}: Enemy Animator or Controller is missing.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (!_animator.HasState(0, _idleStateHash)
            || !_animator.HasState(0, _attackStateHash)
            || !_animator.HasState(0, _attackPoseStateHash)
            || !_animator.HasState(0, _hitStateHash)
            || !_animator.HasState(0, _hitRecoverStateHash))
        {
            Debug.LogError($"{name}: Enemy Controller requires Idle, Attack, AttackPose, Hit and HitRecover states.", this);
            enabled = false;
            return;
        }

        _isReady = true;
        PlayIdle();
    }

    private void OnEnable()
    {
        if (!_isReady)
        {
            return;
        }

        PlayIdle();
    }

    private void OnDisable()
    {
        InterruptAnimation();
    }

    private void InterruptAnimation()
    {
        EAnimationState previousState = _state;
        _state = EAnimationState.Idle;
        _currentAttackCompletionHash = 0;
        _needsIdle = true;
        if (previousState == EAnimationState.Attack)
        {
            OnAttackInterrupted?.Invoke();
        }

        if (previousState == EAnimationState.Hit)
        {
            OnHitInterrupted?.Invoke();
        }
    }

    public void CancelPlayback()
    {
        InterruptAnimation();
        if (_isReady && _animator != null && _animator.isActiveAndEnabled)
        {
            PlayIdle();
        }
    }

    private void LateUpdate()
    {
        if (_needsIdle && IsReady)
        {
            PlayIdle();
        }

        if (_state != EAnimationState.Idle
            && (_animator == null
            || !_animator.isActiveAndEnabled
            || _animator.runtimeAnimatorController == null))
        {
            InterruptAnimation();
            return;
        }

        switch (_state)
        {
            case EAnimationState.Attack:
                if (!HasFinished(_currentAttackCompletionHash))
                {
                    return;
                }

                PlayIdle();
                OnAttackCompleted?.Invoke();
                break;
            case EAnimationState.Hit:
                if (!HasFinished(_hitRecoverStateHash))
                {
                    return;
                }

                PlayIdle();
                OnHitCompleted?.Invoke();
                break;
        }
    }

    public bool TryPlayAttack()
    {
        return TryPlayAttackStates(_attackStateHash, _attackPoseStateHash);
    }

    public bool TryPlayAttack(CSkillDataSO skill)
    {
        if (!TryGetAttackStates(skill, out int attackHash, out int completionHash))
        {
            return false;
        }

        return TryPlayAttackStates(attackHash, completionHash);
    }

    public bool CanPlayAttack(CSkillDataSO skill)
    {
        return TryGetAttackStates(skill, out _, out _);
    }

    public CEnemySkillFeedbackSO GetFeedback(CSkillDataSO skill)
    {
        if (skill == null || _skillAnimations == null) return null;
        for (int i = 0; i < _skillAnimations.Count; i++)
        {
            CEnemySkillAnimation animation = _skillAnimations[i];
            if (animation != null && animation.Skill == skill)
            {
                return animation.Feedback;
            }
        }

        return null;
    }

    public bool TryGetAttackRecovery(out float progress, out float remaining)
    {
        progress = 0.0f;
        remaining = 0.0f;
        if (!IsReady || !IsAttacking) return false;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (info.fullPathHash != _currentAttackCompletionHash) return false;
        progress = Mathf.Clamp01(info.normalizedTime);
        remaining = Mathf.Max(0.0f, info.length * (1.0f - progress) / Mathf.Max(0.01f, _animator.speed));
        return true;
    }

    private bool TryGetAttackStates(CSkillDataSO skill, out int attackHash, out int completionHash)
    {
        attackHash = 0;
        completionHash = 0;
        if (!CanPlayAnimation() || skill == null || _skillAnimations == null)
        {
            return false;
        }

        for (int i = 0; i < _skillAnimations.Count; i++)
        {
            CEnemySkillAnimation animation = _skillAnimations[i];
            if (animation == null || animation.Skill != skill)
            {
                continue;
            }

            // 중복 연결된 스킬 재생 막기
            for (int j = i + 1; j < _skillAnimations.Count; j++)
            {
                if (_skillAnimations[j] != null && _skillAnimations[j].Skill == skill)
                {
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(animation.AttackState) || string.IsNullOrWhiteSpace(animation.CompletionState))
            {
                return false;
            }

            attackHash = Animator.StringToHash(animation.AttackState);
            completionHash = Animator.StringToHash(animation.CompletionState);
            return CanPlayAttackStates(attackHash, completionHash);
        }

        // 연출이 연결되지 않은 스킬 재생 막기
        return false;
    }

    private bool CanPlayAttackStates(int attackHash, int completionHash)
    {
        if (!CanPlayAnimation())
        {
            return false;
        }

        if (!_animator.HasState(0, attackHash) || !_animator.HasState(0, completionHash))
        {
            return false;
        }

        if (attackHash == _idleStateHash
            || completionHash == _idleStateHash
            || attackHash == _hitStateHash
            || completionHash == _hitStateHash
            || attackHash == _hitRecoverStateHash
            || completionHash == _hitRecoverStateHash)
        {
            return false;
        }

        return true;
    }

    private bool TryPlayAttackStates(int attackHash, int completionHash)
    {
        if (!CanPlayAttackStates(attackHash, completionHash))
        {
            return false;
        }

        _currentAttackCompletionHash = completionHash;
        _needsIdle = false;
        _state = EAnimationState.Attack;
        _animator.Play(attackHash, 0, 0.0f);
        OnAttackStarted?.Invoke();
        return true;
    }

    public bool CanPlayHit()
    {
        return CanPlayAnimation() && _animator.HasState(0, _hitStateHash) && _animator.HasState(0, _hitRecoverStateHash);
    }

    public bool TryPlayHit()
    {
        if (!CanPlayHit())
        {
            return false;
        }

        _needsIdle = false;
        _state = EAnimationState.Hit;
        _animator.Play(_hitStateHash, 0, 0.0f);
        OnHitStarted?.Invoke();
        return true;
    }

    private bool CanPlayAnimation()
    {
        return IsReady && !IsBusy;
    }

    private bool HasFinished(int stateHash)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.fullPathHash == stateHash && stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(0);
    }

    private void PlayIdle()
    {
        _state = EAnimationState.Idle;
        _currentAttackCompletionHash = 0;
        _needsIdle = true;
        if (IsReady)
        {
            _animator.Play(_idleStateHash, 0, 0.0f);
            _needsIdle = false;
        }
    }
}
