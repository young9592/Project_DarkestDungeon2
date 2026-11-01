using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CBasicCombatManager : MonoBehaviour
{
    // 라운드 상태
    public enum RoundState
    {
        None,
        Wait,           // 엔티티 생성 대기
        Ready,          // 엔티티 턴순서 준비
        Start           // 엔티티 행동
    }

    // 추가턴인지 아닌지 체크
    public enum TurnState
    {
        Normal,         // 일반턴
        Extra           // 추가턴
    }
    // 현재 엔티티 상태
    public enum EntityState
    {
        Success,
        Dead,
        Stun,
        Daze,
        MeltDown,
    }


    #region Inspector


    [Header("Party")]
    [SerializeField] private CHeroParty _heroParty;
    [SerializeField] private CEnemyParty _enemyParty;

    [Header("Combat UI")]
    [SerializeField] private CCombatUI _combatUI;

    [Header("MessageManager")]
    [SerializeField] private CCombatFloatingMessageManager _messageManager;

    [Header("Debug Menu")]
    [SerializeField] private CCombatDebugMenu _debugMenu;
    [SerializeField] private bool _debugMode = true;

    [Header("CombatManager State")]
    // 현재 필드에 서있는 엔티티 리스트
    [SerializeField] private List<CEntity> _entityList;
    // 현재 라운드 상태
    [SerializeField] private RoundState _roundState = RoundState.None;

    [Header("Option")]
    [SerializeField] private float _actionDelay = 3.5f;
    [SerializeField] private float _portraitRemoveDelay = 0.05f;
    #endregion

    #region Field
    // 상태 리스트
    private Dictionary<CEntity, CEntityState> _entityStateList = new Dictionary<CEntity, CEntityState>();
    // 순서 리스트
    private List<(CEntity entity, int turnSpeed)> _turnList = new List<(CEntity, int)>();
    // 초상화 리스트
    private List<GameObject> _turnPortraitList = new List<GameObject>();
    private GameObject _myTurnHighlight = null;
    private Coroutine _coroutine = null;
    private TurnState _currentTurnState = TurnState.Normal;
    // 행동 완료 알림
    private bool _actionCompleted = false;
    // 인스펙터 널 체크
    private List<Object> _checkList = new List<Object>();
    #endregion

    #region Property
    public List<CEntity> EntityList => _entityList;
    public Dictionary<CEntity, CEntityState> EntityStateList => _entityStateList;
    public CEntity MyTurn => _turnList[0].entity;
    #endregion

    private void Awake()
    {
        #region Null Check
        _checkList.Add(_heroParty);
        _checkList.Add(_enemyParty);
        _checkList.Add(_combatUI);
        _checkList.Add(_messageManager);

        if (_debugMode)
        {
            _checkList.Add(_debugMenu);
        }

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
        {
            enabled = false;
            return;
        }
        #endregion
    }

    private void Update()
    {
        ChangeState();
        ActiveState();
    }
    // 라운드 상태 변경
    private void ChangeState()
    {
        if (_roundState == RoundState.None)
        {
            _roundState = RoundState.Wait;
        }
        // 히어로와 적들의 파티가 생성 완료 되었는가?
        else if (_roundState == RoundState.Wait)
        {
            if (_entityList.Count == _entityStateList.Count)
            {
                _roundState = RoundState.Ready;
            }
        }
        // 턴순서 리스트가 준비가 되었는가?
        else if (_roundState == RoundState.Ready)
        {
            if (_turnList.Count != 0)
            {
                _roundState = RoundState.Start;
            }
        }
        // 턴순서 리스트가 모두 비었는가?
        else if (_roundState == RoundState.Start)
        {
            if (_turnList.Count == 0)
            {
                _roundState = RoundState.Ready;
            }
        }
    }
    // 라운드 상태에 따른 행동
    private void ActiveState()
    {
        switch (_roundState)
        {
            case RoundState.Wait:
                if (CHeroParty.IsReady && CEnemyParty.IsReady)
                {
                    for (int i = 0; i < _heroParty.HeroList.Count; i++)
                    {
                        _entityList.Add(_heroParty.HeroList[i]);
                    }

                    for (int i = 0; i < _enemyParty.EnemyList.Count; i++)
                    {
                        _entityList.Add(_enemyParty.EnemyList[i]);
                    }

                    for (int i = 0; i < _entityList.Count; i++)
                    {
                        _entityStateList.Add(_entityList[i], _combatUI.CreateStateUI(_entityList[i]));
                    }

                    // 디버그 모드면 디버그 메뉴 추가
                    if (_debugMode)
                    {
                        _debugMenu.Initialize();
                    }
                }
                break;
            case RoundState.Ready:
                BuildTurnOrderList();
                break;
            case RoundState.Start:
                ActionEntity();
                break;
        }
    }
    private void BuildTurnOrderList()
    {
        // 모션이 끝날때까지 기다리기
        if (_coroutine != null)
        {
            return;
        }

        _currentTurnState = TurnState.Normal;

        for (int i = 0; i < _entityList.Count; i++)
        {
            // 보스 개체는 행동 횟수를 여러번 가집니다.
            for (int actions = 0; actions < _entityList[i].EntityData.Actions; actions++)
            {
                // 스피드 토큰을 가지고 있을시 행동횟수 1번 속도 +100 보정
                bool hasToken = _entityList[i].TokenManager.ConsumeToken(EEffectType.Speed);

                if (hasToken)
                {
                    _messageManager.ShowMessage(_entityList[i].gameObject, CCombatFloatingText.EMessageType.Speed, "속도!");
                }

                _entityList[i].RoundRandomSpeed();
                _turnList.Add((_entityList[i], _entityList[i].Speed + (hasToken ? 100 : 0)));
            }
        }

        // 선택정렬
        for (int i = 0; i < _turnList.Count - 1; i++)
        {
            (int index, int speed) max = (i, _turnList[i].turnSpeed);

            for (int j = i + 1; j < _turnList.Count; j++)
            {
                if (max.speed < _turnList[j].turnSpeed)
                {
                    // 최소나 최대를 찾아서 체크
                    max = (j, _turnList[j].turnSpeed);
                }
            }

            // 순서 변경
            (_turnList[i], _turnList[max.index]) = (_turnList[max.index], _turnList[i]);
        }

        // 턴 이미지 세팅 및 생성
        _turnPortraitList = _combatUI.CreateTurnOrderPortraitList(_turnList);

        // Debug
        for (int i = 0; i < _turnList.Count; i++)
        {
            CPrint.Log($"index[{i}] :  [{_turnList[i].entity.name} / {_turnList[i].turnSpeed}]");
        }

        // 라운드 올리기
        _combatUI.NextRound(_turnList.Count);
    }
    private void ActionEntity()
    {
        if (_coroutine != null)
        {
            return;
        }

        if (_turnList.Count == 0)
        {
            return;
        }

        if (_myTurnHighlight != null)
        {
            Destroy(_myTurnHighlight);
        }

        // TODO : 자신턴에 자신을 치유하는 모션을 적용하니 변경되었습니다. 수정 필요 
        _myTurnHighlight = _messageManager.ShowIcon(MyTurn.gameObject, CCombatFloatingIcon.IconType.Turn);

        // 행동하기전 상태 체크
        EntityState entityState = CheckState();

        // 상태에 맞는 행동 실행
        switch (entityState)
        {
            case EntityState.Success:
                _coroutine = StartCoroutine(CoAction());
                break;
            case EntityState.Dead:
                _coroutine = StartCoroutine(CoDeath());
                break;
            case EntityState.Stun:
                _coroutine = StartCoroutine(CoStun());
                break;
            case EntityState.Daze:
                _coroutine = StartCoroutine(CoDaze());
                break;
                // TODO : 멘탈붕괴에 대한 행동처리 필요

        }
    }
    // 엔티티 상태 체크
    private EntityState CheckState()
    {
        if (_currentTurnState == TurnState.Normal)
        {
            DotProcess();
        }

        if (MyTurn.IsDead)
        {
            return EntityState.Dead;
        }

        // TODO : 스트레스 받았을 시 연출 추가
        bool isMeltDown = false;

        if (isMeltDown)
        {

        }

        bool isStuned = MyTurn.TokenManager.ConsumeToken(EEffectType.Stun);

        if (isStuned)
        {
            return EntityState.Stun;
        }

        bool isDazed = MyTurn.TokenManager.ConsumeToken(EEffectType.Daze);

        if (isDazed)
        {
            return EntityState.Daze;
        }

        return EntityState.Success;
    }
    private void DotProcess()
    {
        // DOT 데미지 들어오는거 체크 후 죽음 상태로 변경
        bool ishero = MyTurn is CPlayer;

        (int bleed, int blight, int burn) total = (0, 0, 0);

        if (ishero)
        {
            CPlayer hero = MyTurn as CPlayer;
            // 현재 턴에 들어올 데미지 가져오기
            total = (hero.GetTotalDotDamageByType(EEffectType.Bleed), hero.GetTotalDotDamageByType(EEffectType.Blight), hero.GetTotalDotDamageByType(EEffectType.Burn));
            // 도트 데미지 적용
            hero.OnTurnStart();
        }
        else
        {
            CEnemy enemy = MyTurn as CEnemy;
            total = (enemy.GetTotalDotDamageByType(EEffectType.Bleed), enemy.GetTotalDotDamageByType(EEffectType.Blight), enemy.GetTotalDotDamageByType(EEffectType.Burn));
            enemy.OnTurnStart();
        }

        int[] dotList = new int[] { total.bleed, total.blight, total.burn };

        CCombatFloatingText.EMessageType[] dotType = new CCombatFloatingText.EMessageType[] { CCombatFloatingText.EMessageType.Bleed, CCombatFloatingText.EMessageType.Blight, CCombatFloatingText.EMessageType.Burn };

        for (int i = 0, delay = 1; i < dotList.Length; i++)
        {
            if (dotList[i] == 0)
            {
                continue;
            }
            _messageManager.ShowMessage(_turnList[0].entity.gameObject, dotType[i], dotList[i].ToString(), default, delay * 0.3f);
            delay++;
        }
    }
    // 턴이 왔는데 도트뎀으로 죽은 적들 처리
    private IEnumerator CoDeath()
    {
        yield return new WaitForSeconds(_actionDelay);
        for (int i = 0; i < _entityList.Count; i++)
        {
            if (_turnList[0].entity.Equals(_entityList[i]))
            {
                RemoveDeathEntity(i);
                break;
            }
        }
        // 디버그용 드롭박스
        _debugMenu.SetDropboxEntityList();
        _combatUI.MoveTurnOrder(_turnPortraitList, true);
        _coroutine = null;
    }
    // 엔티티끼리 상호작용하고 죽은 엔티티를 확인
    private void CheckEntitiesDeath()
    {
        int prev = _entityList.Count;

        for (int i = _entityList.Count - 1; i >= 0; i--)
        {
            if (_entityList[i].IsDead)
            {
                RemoveDeathEntity(i);
            }
        }
        if (_entityList.Count != prev)
        {
            // 디버그용 드롭박스
            _debugMenu.SetDropboxEntityList();
            _combatUI.MoveTurnOrder(_turnPortraitList, true);
        }
    }
    // 죽은 적 리스트에서 제거
    private void RemoveDeathEntity(int index)
    {
        CEntity entity = _entityList[index];

        // 턴 리스트와 턴순서 초상화 리스트에서 죽은 개체 확인
        for (int i = _turnList.Count - 1; i >= 0; i--)
        {
            // 동일한 엔티티인지 확인
            if (entity.Equals(_turnList[i].entity))
            {
                _turnList.RemoveAt(i);
                Destroy(_turnPortraitList[i], _portraitRemoveDelay);
                _turnPortraitList.RemoveAt(i);
            }
        }
        // TODO : 죽으면 원래 시체가 남기 때문에 임시 구현
        // 시체를 해당 위치에 생성하는 방식도 필요.
        _messageManager.ShowIcon(_entityList[index].gameObject, CCombatFloatingIcon.IconType.DeathStemp);
        _messageManager.ShowMessage(_entityList[index].gameObject, CCombatFloatingText.EMessageType.None, "죽음의 일격!");
        Destroy(_entityList[index].gameObject, _portraitRemoveDelay);
        _entityStateList.Remove(_entityList[index]);
        _entityList.RemoveAt(index);
    }

    // 기절 : 현재 라운드의 차례를 넘깁니다.
    private IEnumerator CoStun()
    {
        _messageManager.ShowMessage(_turnList[0].entity.gameObject, CCombatFloatingText.EMessageType.Stun, "기절!");
        _turnList.RemoveAt(0);
        yield return new WaitForSeconds(_actionDelay);
        _combatUI.MoveTurnOrder(_turnPortraitList);
        Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
        _turnPortraitList.RemoveAt(0);
        _coroutine = null;
    }
    // 현기증 : 자신의 차례를 맨 뒤로 보냅니다. 2중첩시 기절로 변경됩니다.
    private IEnumerator CoDaze()
    {
        _messageManager.ShowMessage(_turnList[0].entity.gameObject, CCombatFloatingText.EMessageType.Daze, "현기증!");

        _turnList.Add(_turnList[0]);
        _turnList.RemoveAt(0);
        yield return new WaitForSeconds(_actionDelay);
        _turnPortraitList.Add(_combatUI.CopyTurnOrderPortrait(_turnPortraitList[0]));
        _combatUI.MoveTurnOrder(_turnPortraitList);
        _turnPortraitList.RemoveAt(0);
        _coroutine = null;
    }
    // 스트레스 : 영웅의 기상 혹은 붕괴 발현
    private IEnumerator CoStress()
    {
        yield return new WaitForSeconds(_actionDelay);
    }
    // 행동
    private IEnumerator CoAction()
    {
        bool doAction = false;

        OpenTurnUI();

        // Could : 턴 시작 시 추가 행동 악세사리 확인

        // 액션할때까지 기다리기  
        while (!doAction)
        {
            if (_combatUI.Pass)
            {
                break;
            }

            
            // TODO : 특정 행동을 하였을 때 true로 변경
            doAction = Keyboard.current.aKey.isPressed;

            yield return null;
        }

        _messageManager.ShowMessage(MyTurn.gameObject, CCombatFloatingText.EMessageType.None, !_combatUI.Pass ? "행동" : "턴넘김");
        _actionCompleted = false;

        yield return new WaitForSeconds(_actionDelay);

        bool _isAfterDead = MyTurn.IsDead;

        if (!_isAfterDead)
        {
            _turnList[0].entity.TryBeginAction();
            // Could :추가 턴 악세사리 여부 체크

            // 턴 상태 체크
            CheckTurnState();
        }

        CheckEntitiesDeath();
        _combatUI.Pass = false;
        _coroutine = null;
    }
    // 현재 턴 행동시 UI 오픈
    private void OpenTurnUI()
    {
        _combatUI.CloseHeroInfomation();
        _combatUI.CloseEnemyInfomation();

        bool isHero = MyTurn is CPlayer;

        if (isHero)
        {
            _combatUI.OpenHeroInfomation((CPlayer)MyTurn);
        }
        // 적의 턴인 경우
        else
        {
            _combatUI.OpenEnemyInfomation((CEnemy)MyTurn, false);
        }
    }
    // 추가 행동 체크
    private void CheckTurnState()
    {
        if (_turnList[0].entity.IsDead)
        {
            return;
        }

        if (_currentTurnState == TurnState.Normal)
        {
            if (_turnList[0].entity.ExtraAction)
            {
                _turnList[0].entity.SetExtraAction(false);
                _currentTurnState = TurnState.Extra;
            }
            else
            {
                _turnList.RemoveAt(0);
                _combatUI.MoveTurnOrder(_turnPortraitList);
                Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
                _turnPortraitList.RemoveAt(0);
            }
        }
        // 추가행동 후 턴에선 
        else if (_currentTurnState == TurnState.Extra)
        {
            _turnList.RemoveAt(0);
            _combatUI.MoveTurnOrder(_turnPortraitList);
            Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
            _turnPortraitList.RemoveAt(0);
            _currentTurnState = TurnState.Normal;
        }
    }
}

//Could 시스템
#region ExtraActionJewelry
/*
// Could : 아무래도 아직 악세사리나 특정조건이 없다보니 임시로 만들었습니다.
private void CheckAddExtraTurn(CTempEntity.EJewelryType type)
{
    if (_turnList[0].entity.IsDead)
    {
        return;
    }

    if (_turnList[0].entity.EquipJewelry == type && _currentTurnState == TurnState.Normal)
    {
        _turnList[0].entity.HasExtraTurn = true;
        ShowUIMessage(_turnList[0].entity.gameObject, CCombatFloatingText.EMessageType.None, "추가 행동");
    }
}
*/
#endregion