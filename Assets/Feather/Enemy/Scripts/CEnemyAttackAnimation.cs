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
    [Header("Hit Recovery")]
    [SerializeField, Range(0.0f, 1.0f)] private float _hitRecoveryExitTime = 1.0f;
    [SerializeField, Min(0.0f)] private float _hitRecoveryBlendTime;
    [SerializeField] private bool _useSourceHitRecovery;
    [SerializeField, Min(0.0f)] private float _sourceHitRecoveryBlendTime;
    private bool _hasPausedSourceHit;
    private bool _hasStartedSourceHitRecovery;
    private bool _hasCompletedSourceHit;
    private int _sourceHitFrame;
    private int _sourceHitRecoveryFrame;
    private static readonly int _idleStateHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int _attackStateHash = Animator.StringToHash("Base Layer.Attack");
    private static readonly int _attackPoseStateHash = Animator.StringToHash("Base Layer.AttackPose");
    private static readonly int _hitStateHash = Animator.StringToHash("Base Layer.Hit");
    private static readonly int _hitRecoverStateHash = Animator.StringToHash("Base Layer.HitRecover");
    private bool _isReady;
    private EAnimationState _state;
    private bool _needsIdle;
    private bool _isHitRecoveryBlending;
    private int _currentAttackCompletionHash;
    private int _currentAttackHash;
    private CEnemySkillAnimation _activeAnimation;
    private bool _hasStartedRecovery;
    private bool _isSourceIdleBlending;
    private float _attackClipLength;
    private float _recoveryClipLength;
    private readonly List<AnimatorClipInfo> _clipInfos = new List<AnimatorClipInfo>();
    // 비활성화로 중단되면 완료 알림 생략
    public event Action OnAttackStarted;
    public event Action OnAttackRecoveryStarted;
    public event Action OnAttackCompleted;
    public event Action OnHitStarted;
    public event Action OnHitCompleted;
    public event Action OnAttackInterrupted;
    public event Action OnHitInterrupted;
    public bool IsAttacking
    {
        get { return _state == EAnimationState.Attack; }
    }

    public bool UsesSourceTiming
    {
        get { return _activeAnimation != null && _activeAnimation.UseSourceTiming; }
    }

    public CSkillDataSO ActiveSkill
    {
        get { return _activeAnimation != null ? _activeAnimation.Skill : null; }
    }

    public bool IsHitReacting
    {
        get { return _state == EAnimationState.Hit && !(_useSourceHitRecovery && _hasCompletedSourceHit); }
    }

    public bool IsBusy
    {
        get { return _state != EAnimationState.Idle && !(_useSourceHitRecovery && _state == EAnimationState.Hit && _hasCompletedSourceHit); }
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

        if (previousState == EAnimationState.Hit && !(_useSourceHitRecovery && _hasCompletedSourceHit))
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

    public void CompleteHitForDeath()
    {
        if (!IsHitReacting)
        {
            return;
        }

        PlayIdle();
        OnHitCompleted?.Invoke();
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
                UpdateSourceRecovery();
                if (!HasFinishedAttack())
                {
                    return;
                }

                PlayIdle();
                OnAttackCompleted?.Invoke();
                break;
            case EAnimationState.Hit:
                UpdateSourceHitRecovery();
                if (_state != EAnimationState.Hit) return;
                if (!TryCompleteHitRecovery())
                {
                    return;
                }

                _state = EAnimationState.Idle;
                if (!(_useSourceHitRecovery && _hasCompletedSourceHit)) OnHitCompleted?.Invoke();
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

        for (int i = 0; i < _skillAnimations.Count; i++)
        {
            if (_skillAnimations[i] != null && _skillAnimations[i].Skill == skill)
                return TryPlayAttackStates(attackHash, completionHash, _skillAnimations[i]);
        }
        return false;
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
        bool isNextState = false;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (info.fullPathHash != _currentAttackCompletionHash)
        {
            if (_activeAnimation == null || !_activeAnimation.UseSourceTiming || !_animator.IsInTransition(0)) return false;
            isNextState = true;
            info = _animator.GetNextAnimatorStateInfo(0);
            if (info.fullPathHash != _currentAttackCompletionHash) return false;
        }
        progress = Mathf.Clamp01(info.normalizedTime);
        if (_activeAnimation != null && _activeAnimation.UseSourceTiming)
        {
            float length = GetRecoveryClipLength(isNextState);
            remaining = _animator.speed > 0.0f
                ? Mathf.Max(0.0f, length * (1.0f - progress) / _animator.speed) : 0.0f;
        }
        else remaining = Mathf.Max(0.0f, info.length * (1.0f - progress) / Mathf.Max(0.01f, _animator.speed));
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

    private bool TryPlayAttackStates(int attackHash, int completionHash, CEnemySkillAnimation animation = null)
    {
        if (!CanPlayAttackStates(attackHash, completionHash))
        {
            return false;
        }

        _currentAttackHash = attackHash;
        _activeAnimation = animation;
        _hasStartedRecovery = false;
        _isSourceIdleBlending = false;
        _attackClipLength = 0.0f;
        _recoveryClipLength = 0.0f;
        _currentAttackCompletionHash = completionHash;
        _needsIdle = false;
        _state = EAnimationState.Attack;
        if (animation != null && animation.UseSourceTiming)
        {
            // 준비 구간을 다시 틀지 않고 원본 공격 진입 위치부터 재생함
            _animator.PlayInFixedTime(attackHash, 0, animation.AttackClipStartTime);
            _animator.Update(0.0f);
            _animator.GetCurrentAnimatorClipInfo(0, _clipInfos);
            if (_clipInfos.Count > 0) _attackClipLength = _clipInfos[0].clip.length;
            if (animation.RecoveryStartDelay <= 0.0f) BeginSourceRecovery();
        }
        else _animator.Play(attackHash, 0, 0.0f);
        OnAttackStarted?.Invoke();
        return true;
    }

    private bool HasFinishedAttack()
    {
        if (_activeAnimation == null || !_activeAnimation.UseSourceTiming
            || _activeAnimation.RecoveryClipEndTime <= 0.0f) return HasFinished(_currentAttackCompletionHash);
        if (_animator.speed <= 0.0f) return false;

        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (_isSourceIdleBlending)
        {
            return info.fullPathHash == _idleStateHash && !_animator.IsInTransition(0);
        }
        if (_animator.IsInTransition(0) || info.fullPathHash != _currentAttackCompletionHash) return false;

        float elapsed = info.normalizedTime * GetRecoveryClipLength(false);
        float blendTime = Mathf.Min(_activeAnimation.RecoveryIdleBlendTime,
            _activeAnimation.RecoveryClipEndTime - _activeAnimation.RecoveryClipStartTime);
        if (blendTime > 0.0f && elapsed >= _activeAnimation.RecoveryClipEndTime - blendTime)
        {
            // 복귀 마지막 구간은 원본처럼 대기 자세와 섞어서 마무리함
            _isSourceIdleBlending = true;
            _animator.CrossFadeInFixedTime(_idleStateHash, blendTime, 0, 0.0f);
            return false;
        }
        return elapsed >= _activeAnimation.RecoveryClipEndTime;
    }

    private float GetRecoveryClipLength(bool isNextState)
    {
        if (_recoveryClipLength > 0.0f) return _recoveryClipLength;
        // 정지 중에는 StateInfo.length가 무한대가 될 수 있어 원본 클립 길이를 씀
        _clipInfos.Clear();
        if (isNextState) _animator.GetNextAnimatorClipInfo(0, _clipInfos);
        else _animator.GetCurrentAnimatorClipInfo(0, _clipInfos);
        if (_clipInfos.Count > 0 && _clipInfos[0].clip != null) _recoveryClipLength = _clipInfos[0].clip.length;
        return _recoveryClipLength;
    }

    private void UpdateSourceRecovery()
    {
        if (_activeAnimation == null || !_activeAnimation.UseSourceTiming || _hasStartedRecovery
            || _animator.speed <= 0.0f) return;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (info.fullPathHash != _currentAttackHash) return;
        float elapsed = info.normalizedTime * _attackClipLength - _activeAnimation.AttackClipStartTime;
        if (elapsed >= _activeAnimation.RecoveryStartDelay) BeginSourceRecovery();
    }

    private void BeginSourceRecovery()
    {
        if (_hasStartedRecovery || _activeAnimation == null) return;
        _hasStartedRecovery = true;
        _animator.CrossFadeInFixedTime(_currentAttackCompletionHash,
            _activeAnimation.RecoveryBlendTime, 0, _activeAnimation.RecoveryClipStartTime);
        OnAttackRecoveryStarted?.Invoke();
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
        _isHitRecoveryBlending = false;
        _hasPausedSourceHit = _animator.speed <= 0.0f;
        _hasStartedSourceHitRecovery = false;
        _hasCompletedSourceHit = false;
        _sourceHitFrame = Time.frameCount;
        _state = EAnimationState.Hit;
        _animator.Play(_hitStateHash, 0, 0.0f);
        OnHitStarted?.Invoke();
        return true;
    }

    private void UpdateSourceHitRecovery()
    {
        if (!_useSourceHitRecovery) return;
        if (!_hasStartedSourceHitRecovery)
        {
            if (_animator.speed <= 0.0f)
            {
                _hasPausedSourceHit = true;
                return;
            }
            // 확대 중에는 피격 자세를 유지하고, 재생이 풀리면 원본 복귀로 넘어감
            int highlightLayer = LayerMask.NameToLayer("Highlight");
            if (!_hasPausedSourceHit && (Time.frameCount <= _sourceHitFrame || gameObject.layer == highlightLayer)) return;
            _hasStartedSourceHitRecovery = true;
            _sourceHitRecoveryFrame = Time.frameCount;
            _animator.CrossFadeInFixedTime(_hitRecoverStateHash, _sourceHitRecoveryBlendTime, 0, 0.0f);
            return;
        }
        if (_hasCompletedSourceHit || Time.frameCount <= _sourceHitRecoveryFrame) return;

        // 원본처럼 복귀를 시작한 다음 프레임에 전투 대기만 풀고, 남은 모션은 계속 재생함
        _hasCompletedSourceHit = true;
        OnHitCompleted?.Invoke();
    }

    private bool CanPlayAnimation()
    {
        return IsReady && !IsBusy;
    }

    private bool TryCompleteHitRecovery()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (_isHitRecoveryBlending && stateInfo.fullPathHash == _idleStateHash)
        {
            return !_animator.IsInTransition(0);
        }

        if (_animator.IsInTransition(0) || stateInfo.fullPathHash != _hitRecoverStateHash
            || stateInfo.normalizedTime < _hitRecoveryExitTime)
        {
            return false;
        }

        if (_hitRecoveryBlendTime > 0.0f)
        {
            _isHitRecoveryBlending = true;
            _animator.CrossFadeInFixedTime(_idleStateHash, _hitRecoveryBlendTime, 0, 0.0f);
            return false;
        }

        PlayIdle();
        return true;
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
            if (!_isSourceIdleBlending || _animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _idleStateHash)
            {
                _animator.Play(_idleStateHash, 0, 0.0f);
            }
            _isSourceIdleBlending = false;
            _needsIdle = false;
        }
    }
}
