using System;
using UnityEngine;

public enum EEntityType
{
    None,
    Hero,
    Enemy
}

public enum EEntityActionState
{
    Idle,
    Selecting,
    Requested,
    Executing
}
public abstract class CEntity : MonoBehaviour
{
    [SerializeField] protected bool _isDeathsDoor = false;
    [SerializeField] protected bool _isDead = false;

    private int _roundSpeedBonus;
    private bool _usesActionLifecycle;
    private EEntityActionState _actionState;
    private CEntity _actionTarget;
    private CSkillDataSO _actionSkill;
    private bool _extraActions = false;  // 추가행동

    public virtual CEntityDataSO EntityData { get { return null; } }
    public virtual EEntityType EntityType { get { return EEntityType.None; } }
    public virtual ERank CurrentRank { get { return ERank.None; } }
    public int BaseSpeed { get { return EntityData != null ? EntityData.Speed : 0; } }
    public int Speed { get { return BaseSpeed + _roundSpeedBonus; } }
    public EEntityActionState ActionState { get { return _actionState; } }
    public bool IsActionActive { get { return _actionState != EEntityActionState.Idle; } }
    public CEntity ActionTarget { get { return _actionTarget; } }
    public CSkillDataSO ActionSkill { get { return _actionSkill; } }
    public virtual void CureStatusEffect(EEffectType targetType = EEffectType.None) { }
    public CTokenManager TokenManager { get; private set ; }
    public virtual void ResetEntityState() { }

    public bool IsDeathsDoor 
    { 
        get { return _isDeathsDoor; } 
        protected set { _isDeathsDoor = value; }
    }
    public bool IsDead 
    { 
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    public bool ExtraAction => _extraActions;

    public void SetExtraAction(bool setState)
    {
        _extraActions = setState;
    }

    protected virtual void Awake()
    {
        // 생성될 때 토큰 주머니 생성
        TokenManager = new CTokenManager();
        _isDeathsDoor = false;
        _isDead = false;
    }


    public event Action<CEntity> OnActionStarted;
    public event Action<CEntity, CEntity, CSkillDataSO> OnActionRequested;
    public event Action<CEntity> OnActionCompleted;
    public event Action<CEntity> OnActionInterrupted;

    protected bool HasCommonRequestReceiver
    {
        get { return _actionState == EEntityActionState.Selecting && OnActionRequested != null; }
    }

    protected bool CanIssueActionRequest
    {
        get { return !_usesActionLifecycle || _actionState == EEntityActionState.Selecting; }
    }

    public void RoundRandomSpeed()
    {
        _roundSpeedBonus = UnityEngine.Random.Range(0, 7);
    }

    public virtual bool CanRequestSkillAction(CEntity target, ERank targetRank) { return false; }
    public virtual bool TryRequestSkillAction(CEntity target, ERank targetRank) { return false; }

    public bool TryBeginAction()
    {
        if (!isActiveAndEnabled || EntityData == null || IsActionActive || !CanBeginAction()) return false;
        _usesActionLifecycle = true;
        _actionState = EEntityActionState.Selecting;
        OnActionBeginning();
        OnActionStarted?.Invoke(this);
        return true;
    }

    protected bool NotifyActionRequest(CEntity target, CSkillDataSO skill)
    {
        if (_usesActionLifecycle)
        {
            _actionTarget = target;
            _actionSkill = skill;
            _actionState = EEntityActionState.Requested;
        }
        if (!_usesActionLifecycle || OnActionRequested == null) return false;
        OnActionRequested.Invoke(this, target, skill);
        return true;
    }

    public bool TryStartRequestedAction()
    {
        if (!isActiveAndEnabled || _actionState != EEntityActionState.Requested) return false;
        if (_actionTarget == null || !_actionTarget.isActiveAndEnabled || !CanExecuteRequestedAction())
        {
            CancelAction();
            return false;
        }
        _actionState = EEntityActionState.Executing;
        if (TryStartActionPresentation()) return true;
        CancelAction();
        return false;
    }

    public bool TryCompleteAction()
    {
        if (_actionState != EEntityActionState.Executing || !CanCompleteAction()) return false;
        ResetAction();
        OnActionEnded();
        OnActionCompleted?.Invoke(this);
        return true;
    }

    public bool CancelAction()
    {
        if (!IsActionActive) return false;
        ResetAction();
        OnActionCancelled();
        OnActionEnded();
        OnActionInterrupted?.Invoke(this);
        return true;
    }

    private void ResetAction()
    {
        // 알림을 받는 쪽에서 다시 호출해도 종료가 중복되지 않게 먼저 비웁니다.
        _actionState = EEntityActionState.Idle;
        _actionTarget = null;
        _actionSkill = null;
    }

    protected virtual bool CanBeginAction() { return true; }
    protected virtual bool CanCompleteAction() { return true; }
    protected virtual void OnActionBeginning() { }
    protected virtual void OnActionEnded() { }
    protected virtual void OnActionCancelled() { }
    protected virtual bool CanExecuteRequestedAction() { return _actionSkill != null; }
    protected virtual bool TryStartActionPresentation() { return true; }

    // 스트레스 증감처리
    public virtual void ModifyStress(int stressAmount) { }

    protected virtual void OnDisable()
    {
        CancelAction();
    }


    /// <summary>
    /// Effect Core 파이프라인 연산이 모두 끝난 최종 데미지만 받아 체력 조절시킴
    /// 힐링 스킬 같은 경우에 음수로 데미지 처리해서 처리할 생각하고 있음
    /// </summary>
    public abstract void TakeDamage(int finalDamage);

    /// <summary>
    /// 출혈, 중독, 콤보 토큰 등 턴 기반의 상태이상 부분
    /// Effect Core에서 명중 판정을 끝내면 여기서 유효한 효과만 던지기
    /// </summary>
    public abstract void ApplyStatusEffect(CEffect statusEffect);

    // TODO : 토큰 흭득 / 차감 등도 필요시 여기에 분리해서 추가 할 수도?
}
