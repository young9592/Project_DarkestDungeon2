using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CEntityDeathPresentation : MonoBehaviour
{
    [Header("Death Presentation")]
    [SerializeField] private string _deathState = "Base Layer.Death";
    private CEntity _entity;
    private Animator _animator;
    private CHeroAnimation _heroAnimation;
    private bool _heroAnimationEnabled;
    private bool _isConfirmed;
    private bool _isPlaying;
    private bool _keepAnimatorState;
    private int _presentationVersion;
    private bool _wasHitReacting;

    public bool WasHitReacting
    {
        get { return _wasHitReacting; }
    }

    public bool IsConfirmed
    {
        get { return _isConfirmed; }
    }

    public bool IsBusy
    {
        get { return _isPlaying; }
    }

    public bool HasDeathClip
    {
        get { return _animator != null && _animator.runtimeAnimatorController != null
            && _animator.HasState(0, Animator.StringToHash(_deathState)); }
    }

    public event Action<CEntity> OnDeathPresentationStarted;
    public event Action<CEntity> OnDeathPresentationCompleted;
    public event Action<CEntity> OnDeathPresentationInterrupted;

    private void Awake()
    {
        _entity = GetComponent<CEntity>();
        _animator = GetComponentInChildren<Animator>(true);
        _heroAnimation = GetComponent<CHeroAnimation>();
    }

    private void OnEnable()
    {
        if (_entity is CPlayer hero)
        {
            hero.OnPlayerDied += HandlePlayerDied;
        }

        if (_isConfirmed && HasDeathClip && _animator.isActiveAndEnabled)
        {
            _animator.Play(Animator.StringToHash(_deathState), 0, 1.0f);
        }
    }

    public bool TryPlayConfirmedDeath()
    {
        if (!isActiveAndEnabled || _entity == null || !_entity.isActiveAndEnabled || _isConfirmed)
        {
            return false;
        }

        // 체력으로 판단하지 않고 외부에서 받은 사망 확정만 표시
        _isConfirmed = true;
        int version = ++_presentationVersion;
        _heroAnimationEnabled = _heroAnimation != null && _heroAnimation.enabled;
        _wasHitReacting = _heroAnimation != null && _heroAnimation.IsHitReacting;
        _isPlaying = HasDeathClip && _animator.isActiveAndEnabled;
        if (_animator != null)
        {
            _keepAnimatorState = _animator.keepAnimatorStateOnDisable;
            _animator.keepAnimatorStateOnDisable = true;
        }

        // 피격에서 사망으로 이어질 때 공격자의 완료 대기 유지
        if (_heroAnimation != null)
        {
            _heroAnimation.CompleteHitForDeath();
        }


        _entity.CancelAction();
        if (version != _presentationVersion || !_isConfirmed || !isActiveAndEnabled)
        {
            return true;
        }
        CHeroTargetView targetView = GetComponent<CHeroTargetView>();
        if (targetView != null)
        {
            targetView.SetHighlight(false, false);
        }

        if (_heroAnimation != null)
        {
            _heroAnimation.CancelPlayback();
            _heroAnimation.enabled = false;
        }


        OnDeathPresentationStarted?.Invoke(_entity);
        if (version != _presentationVersion || !_isConfirmed || !isActiveAndEnabled)
        {
            return true;
        }
        if (_isPlaying)
        {
            _animator.Play(Animator.StringToHash(_deathState), 0, 0.0f);
        }
        else
        {
            // 사망 클립이 없는 캐릭터는 선택 해제까지만 처리
            OnDeathPresentationCompleted?.Invoke(_entity);
        }

        return true;
    }

    private void LateUpdate()
    {
        if (!_isPlaying)
        {
            return;
        }

        if (_animator == null || !_animator.isActiveAndEnabled || _animator.runtimeAnimatorController == null)
        {
            _isPlaying = false;
            OnDeathPresentationInterrupted?.Invoke(_entity);
            return;
        }

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash == Animator.StringToHash(_deathState)
            && state.normalizedTime >= 1.0f && !_animator.IsInTransition(0))
        {
            _isPlaying = false;
            OnDeathPresentationCompleted?.Invoke(_entity);
        }
    }

    public void ResetPresentation()
    {
        if (!_isConfirmed)
        {
            return;
        }

        bool interrupted = _isPlaying;
        _presentationVersion++;
        _isPlaying = false;
        _isConfirmed = false;
        _wasHitReacting = false;
        if (_animator != null)
        {
            _animator.keepAnimatorStateOnDisable = _keepAnimatorState;
        }
        if (_heroAnimation != null)
        {
            _heroAnimation.enabled = _heroAnimationEnabled;
            _heroAnimation.CancelPlayback();
        }


        if (interrupted)
        {
            OnDeathPresentationInterrupted?.Invoke(_entity);
        }
    }

    private void HandlePlayerDied(CPlayer hero)
    {
        TryPlayConfirmedDeath();
    }

    private void OnDisable()
    {
        if (_entity is CPlayer hero)
        {
            hero.OnPlayerDied -= HandlePlayerDied;
        }

        _presentationVersion++;
        if (_isPlaying)
        {
            _isPlaying = false;
            OnDeathPresentationInterrupted?.Invoke(_entity);
        }
    }
}
