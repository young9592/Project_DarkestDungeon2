using System;
using System.Collections.Generic;
using UnityEngine;

public class CHeroAnimation : MonoBehaviour
{
    private enum EAnimationState { Idle, Selecting, Attack, Hit }

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private List<CSkillDataSO> _attackSkills = new List<CSkillDataSO>();

    private EAnimationState _state;
    private bool _needsIdle;
    private int _currentAttackEndHash;
    private CSkillDataSO _selectionSkill;
    private int _selectionStartHash;
    private int _selectionHoldHash;
    private bool _waitForSelection;
    private static readonly int _idleHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int _attackHash = Animator.StringToHash("Base Layer.AttackAntic");
    private static readonly int _attackEndHash = Animator.StringToHash("Base Layer.AttackRecover");
    private static readonly int _hitHash = Animator.StringToHash("Base Layer.Hit");
    private static readonly int _hitEndHash = Animator.StringToHash("Base Layer.HitRecover");

    public event Action OnAttackStarted;
    public event Action OnAttackCompleted;
    public event Action OnAttackInterrupted;
    public event Action OnHitStarted;
    public event Action OnHitCompleted;
    public event Action OnHitInterrupted;

    public bool IsBusy { get { return IsAttacking || IsHitReacting; } }
    public bool IsSelecting { get { return _state == EAnimationState.Selecting; } }
    public bool IsAttacking { get { return _state == EAnimationState.Attack; } }
    public bool IsHitReacting { get { return _state == EAnimationState.Hit; } }
    public bool IsReady
    {
        get
        {
            return isActiveAndEnabled && _animator != null && _animator.isActiveAndEnabled
                && _animator.runtimeAnimatorController != null && _animator.HasState(0, _idleHash);
        }
    }

    private void Awake()
    {
        if (_animator == null) _animator = GetComponentInChildren<Animator>(true);
        if (_animator == null || _animator.runtimeAnimatorController == null)
        {
            Debug.LogError(name + ": Hero Animator와 Controller를 연결해야 합니다.", this);
            enabled = false;
        }
    }

    public bool TryPlaySelection(CSkillDataSO skill)
    {
        if (!CanPlayAttack(skill)) return false;
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        if (heroSkill == null || string.IsNullOrEmpty(heroSkill.SelectionStartState)
            || string.IsNullOrEmpty(heroSkill.SelectionHoldState)) return true;

        int startHash = Animator.StringToHash(heroSkill.SelectionStartState);
        int holdHash = Animator.StringToHash(heroSkill.SelectionHoldState);
        if (!_animator.HasState(0, startHash) || !_animator.HasState(0, holdHash)) return false;
        if (IsSelecting && _selectionSkill == skill) return true;

        _needsIdle = false;
        _state = EAnimationState.Selecting;
        _selectionSkill = skill;
        _selectionStartHash = startHash;
        _selectionHoldHash = holdHash;
        _waitForSelection = false;
        _animator.Play(startHash, 0, 0.0f);
        return true;
    }

    public void CancelSelection()
    {
        if (IsSelecting) PlayIdle();
    }

    public bool TryPlayAttack(CSkillDataSO skill)
    {
        if (!CanPlayAttack(skill)) return false;
        bool prepared = IsSelecting && _selectionSkill == skill;
        _needsIdle = false;
        _state = EAnimationState.Attack;
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        int startHash = heroSkill != null ? Animator.StringToHash(heroSkill.StartState) : _attackHash;
        _currentAttackEndHash = heroSkill != null ? Animator.StringToHash(heroSkill.EndState) : _attackEndHash;
        _waitForSelection = prepared;
        _selectionSkill = null;
        if (!prepared)
        {
            _animator.Play(startHash, 0, 0.0f);
        }
        else if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _selectionHoldHash)
        {
            _waitForSelection = false;
            _animator.Play(_currentAttackEndHash, 0, 0.0f);
        }
        OnAttackStarted?.Invoke();
        return true;
    }

    public bool TryPlayHit()
    {
        if (!CanPlayHit()) return false;
        _needsIdle = false;
        _selectionSkill = null;
        _waitForSelection = false;
        _state = EAnimationState.Hit;
        _animator.Play(_hitHash, 0, 0.0f);
        OnHitStarted?.Invoke();
        return true;
    }

    public bool CanPlayAttack(CSkillDataSO skill)
    {
        if (!IsReady || IsBusy || skill == null || _attackSkills == null || !_attackSkills.Contains(skill)) return false;
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        if (heroSkill == null) return _animator.HasState(0, _attackHash) && _animator.HasState(0, _attackEndHash);
        if (string.IsNullOrEmpty(heroSkill.StartState) || string.IsNullOrEmpty(heroSkill.EndState)) return false;
        int startHash = Animator.StringToHash(heroSkill.StartState);
        int endHash = Animator.StringToHash(heroSkill.EndState);
        return startHash != _idleHash && startHash != _hitHash && endHash != _idleHash && endHash != _hitEndHash
            && _animator.HasState(0, startHash) && _animator.HasState(0, endHash);
    }

    public bool CanPlayHit()
    {
        return IsReady && !IsBusy && _animator.HasState(0, _hitHash) && _animator.HasState(0, _hitEndHash);
    }

    public bool TryGetAttackRecovery(out float progress, out float remaining)
    {
        progress = 0.0f;
        remaining = 0.0f;
        if (!IsReady || !IsAttacking) return false;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (info.fullPathHash != _currentAttackEndHash) return false;
        progress = Mathf.Clamp01(info.normalizedTime);
        remaining = Mathf.Max(0.0f, info.length * (1.0f - progress) / Mathf.Max(0.01f, _animator.speed));
        return true;
    }

    private void LateUpdate()
    {
        if (_needsIdle && IsReady) PlayIdle();
        if (_state == EAnimationState.Idle) return;
        if (_animator == null || !_animator.isActiveAndEnabled || _animator.runtimeAnimatorController == null)
        {
            CancelPlayback();
            return;
        }
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (IsSelecting) return;
        if (_waitForSelection)
        {
            bool prepared = info.fullPathHash == _selectionHoldHash
                || (info.fullPathHash == _selectionStartHash && info.normalizedTime >= 1.0f
                && !_animator.IsInTransition(0));
            if (!prepared) return;
            _waitForSelection = false;
            _animator.Play(_currentAttackEndHash, 0, 0.0f);
            return;
        }

        int endHash = IsAttacking ? _currentAttackEndHash : _hitEndHash;
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
        _selectionSkill = null;
        _waitForSelection = false;
        _needsIdle = true;
        if (IsReady)
        {
            _animator.Play(_idleHash, 0, 0.0f);
            _needsIdle = false;
        }
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
