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
    private int _currentAttackCompletionHash;

    // 비활성화로 중단된 연출은 완료 알림을 보내지 않습니다.
    public event Action OnAttackCompleted;
    public event Action OnHitCompleted;

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
        if (!_animator.HasState(0, _idleStateHash) || !_animator.HasState(0, _attackStateHash)
            || !_animator.HasState(0, _attackPoseStateHash) || !_animator.HasState(0, _hitStateHash)
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
        if (!_isReady) return;

        PlayIdle();
    }

    private void OnDisable()
    {
        _state = EAnimationState.Idle;
        _currentAttackCompletionHash = 0;
    }

    private void LateUpdate()
    {
        switch (_state)
        {
            case EAnimationState.Attack:
                if (!HasFinished(_currentAttackCompletionHash)) return;

                PlayIdle();
                OnAttackCompleted?.Invoke();
                break;
            case EAnimationState.Hit:
                if (!HasFinished(_hitRecoverStateHash)) return;

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
        if (!CanPlayAnimation() || skill == null || _skillAnimations == null) return false;

        for (int i = 0; i < _skillAnimations.Count; i++)
        {
            CEnemySkillAnimation animation = _skillAnimations[i];
            if (animation == null || animation.Skill != skill) continue;

            // 중복 연결은 어느 연출을 사용할지 모호하므로 재생하지 않습니다.
            for (int j = i + 1; j < _skillAnimations.Count; j++)
            {
                if (_skillAnimations[j] != null && _skillAnimations[j].Skill == skill) return false;
            }

            if (string.IsNullOrWhiteSpace(animation.AttackState)
                || string.IsNullOrWhiteSpace(animation.CompletionState)) return false;

            return TryPlayAttackStates(Animator.StringToHash(animation.AttackState),
                Animator.StringToHash(animation.CompletionState));
        }

        // 연결되지 않은 스킬을 기본 공격으로 대신 재생하지 않습니다.
        return false;
    }

    private bool TryPlayAttackStates(int attackHash, int completionHash)
    {
        if (!CanPlayAnimation()) return false;
        if (!_animator.HasState(0, attackHash) || !_animator.HasState(0, completionHash)) return false;
        if (attackHash == _idleStateHash || completionHash == _idleStateHash
            || attackHash == _hitStateHash || completionHash == _hitStateHash
            || attackHash == _hitRecoverStateHash || completionHash == _hitRecoverStateHash) return false;

        _currentAttackCompletionHash = completionHash;
        _state = EAnimationState.Attack;
        _animator.Play(attackHash, 0, 0.0f);
        return true;
    }

    public bool TryPlayHit()
    {
        if (!CanPlayAnimation()) return false;

        _state = EAnimationState.Hit;
        _animator.Play(_hitStateHash, 0, 0.0f);
        return true;
    }

    private bool CanPlayAnimation()
    {
        return _isReady && isActiveAndEnabled && !IsBusy && _animator.isActiveAndEnabled;
    }

    private bool HasFinished(int stateHash)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.fullPathHash == stateHash && stateInfo.normalizedTime >= 1.0f
            && !_animator.IsInTransition(0);
    }

    private void PlayIdle()
    {
        _state = EAnimationState.Idle;
        _currentAttackCompletionHash = 0;
        _animator.Play(_idleStateHash, 0, 0.0f);
    }
}
