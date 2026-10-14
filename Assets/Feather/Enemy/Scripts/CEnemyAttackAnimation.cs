using System;
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

    private static readonly int _idleStateHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int _attackStateHash = Animator.StringToHash("Base Layer.Attack");
    private static readonly int _attackPoseStateHash = Animator.StringToHash("Base Layer.AttackPose");
    private static readonly int _hitStateHash = Animator.StringToHash("Base Layer.Hit");
    private static readonly int _hitRecoverStateHash = Animator.StringToHash("Base Layer.HitRecover");

    private bool _isReady;
    private EAnimationState _state;

    // 비활성화로 중단된 연출은 완료 알림을 보내지 않는다.
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
    }

    private void LateUpdate()
    {
        switch (_state)
        {
            case EAnimationState.Attack:
                if (!HasFinished(_attackPoseStateHash)) return;

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
        if (!CanPlayAnimation()) return false;

        _state = EAnimationState.Attack;
        _animator.Play(_attackStateHash, 0, 0.0f);
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
        _animator.Play(_idleStateHash, 0, 0.0f);
    }
}
