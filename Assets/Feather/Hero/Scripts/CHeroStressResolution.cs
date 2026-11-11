using System;
using UnityEngine;

public enum EHeroStressResult
{
    Meltdown,
    Resolute
}

[DisallowMultipleComponent]
public class CHeroStressResolution : MonoBehaviour
{
    private CPlayer _hero;
    private CHeroAnimation _animation;
    private CHeroMeltdownPresentation _presentation;
    private bool _isPending;
    private bool _hasResult;
    private int _requestId;
    private EHeroStressResult _result;

    public bool IsPending
    {
        get { return _isPending; }
    }
    public bool HasResult
    {
        get { return _hasResult; }
    }
    public int RequestId
    {
        get { return _requestId; }
    }

    public event Action<CPlayer> OnRequested;
    public event Action<CPlayer, EHeroStressResult> OnApplied;

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
        _animation = GetComponent<CHeroAnimation>();
        _presentation = GetComponent<CHeroMeltdownPresentation>();
    }

    private void OnEnable()
    {
        if (_hero == null) return;
        _hero.OnStressChanged += HandleStressChanged;
        _hero.OnPlayerDied += HandleDeath;
        if (_animation != null) _animation.OnMeltdownInterrupted += HandleInterrupted;
        CheckPending();
    }

    public void CheckPending()
    {
        if (_hero == null || !isActiveAndEnabled || !_hero.isActiveAndEnabled
            || _hero.IsDead || _hero.CurrentStress < 10)
        {
            ResetState();
            return;
        }

        if (_isPending) return;
        _isPending = true;
        _requestId++;
        if (_hero.ActionState == EEntityActionState.Selecting) _hero.CancelAction();
        OnRequested?.Invoke(_hero);
    }

    public bool TryBeginResult(int requestId, EHeroStressResult result)
    {
        if (!CanReceiveResult(requestId) || _hasResult || _hero.IsPresentationBusy
            || _hero.ActionState == EEntityActionState.Requested
            || _hero.ActionState == EEntityActionState.Executing
            || (result != EHeroStressResult.Meltdown && result != EHeroStressResult.Resolute)) return false;

        if (_hero.ActionState == EEntityActionState.Selecting) _hero.CancelAction();
        if (result == EHeroStressResult.Meltdown
            && (_presentation == null || !_presentation.TryQueue())) return false;

        _result = result;
        _hasResult = true;
        if (result == EHeroStressResult.Resolute && _animation != null) _animation.PlayResolute();
        return true;
    }

    // 카메라 연출까지 끝난 뒤 외부에서 호출하기
    public bool TryApplyResult(int requestId)
    {
        if (!CanReceiveResult(requestId) || !_hasResult
            || _hero.IsPresentationBusy || _hero.IsActionActive) return false;

        EHeroStressResult result = _result;
        _isPending = false;
        _hasResult = false;
        _requestId++;

        if (result == EHeroStressResult.Meltdown) _hero.ApplyMeltdownResult();
        else _hero.ApplyResoluteResult();
        OnApplied?.Invoke(_hero, result);
        return true;
    }

    public bool TryCancelResult(int requestId)
    {
        if (!CanReceiveResult(requestId)) return false;
        bool cancelAnimation = _hasResult && _result == EHeroStressResult.Meltdown;
        _hasResult = false;
        _requestId++;
        if (cancelAnimation && _animation != null) _animation.CancelMeltdown();
        if (_animation != null) _animation.EndResolute();
        return true;
    }

    public void ResetState()
    {
        if (!_isPending && !_hasResult) return;
        bool cancelAnimation = _hasResult && _result == EHeroStressResult.Meltdown;
        _isPending = false;
        _hasResult = false;
        _requestId++;
        if (cancelAnimation && _animation != null) _animation.CancelMeltdown();
        if (_animation != null) _animation.EndResolute();
    }

    private bool CanReceiveResult(int requestId)
    {
        return isActiveAndEnabled && _hero != null && _hero.isActiveAndEnabled
            && !_hero.IsDead && !_hero.IsDeathPresented && _hero.CurrentStress >= 10
            && _isPending && requestId == _requestId;
    }

    private void HandleStressChanged(CPlayer hero, int previousStress, int currentStress)
    {
        CheckPending();
    }

    private void HandleDeath(CPlayer hero)
    {
        ResetState();
    }

    private void HandleInterrupted()
    {
        if (!_isPending || !_hasResult || _result != EHeroStressResult.Meltdown) return;
        _hasResult = false;
        _requestId++;
    }

    private void OnDisable()
    {
        ResetState();
        if (_hero == null) return;
        _hero.OnStressChanged -= HandleStressChanged;
        _hero.OnPlayerDied -= HandleDeath;
        if (_animation != null) _animation.OnMeltdownInterrupted -= HandleInterrupted;
    }
}
