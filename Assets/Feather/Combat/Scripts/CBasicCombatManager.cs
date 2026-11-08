using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBasicCombatManager : MonoBehaviour
{
    #region Enum
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
        Stress,
    }
    #endregion

    #region Inspector
    [Header("Party")]
    [SerializeField] private CHeroParty _heroParty;
    [SerializeField] private CEnemyParty _enemyParty;

    [Header("BattleInput")]
    [SerializeField] private CHeroBattleInput _heroBattleInput;

    [Header("CombatCamera")]
    [SerializeField] private CCombatCameraManager _combatCameraManager;

    [Header("Combat UI")]
    [SerializeField] private CCombatUI _combatUI;

    [Header("MessageManager")]
    [SerializeField] private CCombatFloatingMessageManager _messageManager;

    [Header("Debug Menu")]
    [SerializeField] private CCombatDebugMenu _debugMenu;

    [Header("CombatManager State")]
    // 현재 필드에 서있는 엔티티 리스트
    [SerializeField] private List<(int id, CEntity entity)> _entityList;
    // 현재 라운드 상태
    [SerializeField] private RoundState _roundState = RoundState.None;

    [Header("Option")]
    [SerializeField] private float _actionDelay = 3.5f;
    [SerializeField] private float _turnSkipDelay = 1.5f;
    [SerializeField] private float _portraitRemoveDelay = 0.05f;
    #endregion

    #region Field
    // 상태 리스트
    private Dictionary<CEntity, CEntityState> _entitiesState = new Dictionary<CEntity, CEntityState>();
    // 지시등 리스트
    private Dictionary<CEntity, CIndicator> _entitiesIndicator = new Dictionary<CEntity, CIndicator>();
    // 순서 리스트
    private List<(int id, CEntity entity, int turnSpeed)> _turnList = new List<(int, CEntity, int)>();
    // 초상화 리스트
    private List<GameObject> _turnPortraitList = new List<GameObject>();

    private Coroutine _actionCoroutine = null;
    private Coroutine _sortCoroutine = null;
    private Coroutine _tryBeginCoroutine = null;
    private Coroutine _combatCoroutine = null;
    private Coroutine _multiStressCoroutine = null;
    private TurnState _currentTurnState = TurnState.Normal;
    private bool _doingAction = false;
    private CSkillDataSO _currentActionSkillData;
    private CPlayer _currentStressHero = null;
    private List<CEntity> _corpseCheck = new List<CEntity>();
    private bool _battleEnd = false;
    // 인스펙터 널 체크
    private List<Object> _checkList = new List<Object>();
    #endregion

    #region Property
    public List<(int, CEntity)> EntityList => _entityList;
    public Dictionary<CEntity, CEntityState> EntitiesState => _entitiesState;
    public Dictionary<CEntity, CIndicator> EntitiesIndicator => _entitiesIndicator;
    public CEntity MyTurn => _turnList.Count != 0 ? _turnList[0].entity : null;
    public CIndicator MyTurnIndicator = null;
    public IReadOnlyList<GameObject> HeroRankPosition => _heroParty.HeroRankPosition;
    public IReadOnlyList<GameObject> EnemyRankPosition => _enemyParty.EnemyRankPosition;
    #endregion

    private void Awake()
    {
        #region Null Check
        _checkList.Add(_heroParty);
        _checkList.Add(_enemyParty);
        _checkList.Add(_combatUI);
        _checkList.Add(_messageManager);

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
        {
            enabled = false;
            return;
        }
        #endregion
    }

    private void Update()
    {
        if (_battleEnd)
        {
            return;
        }

        if (_actionCoroutine != null)
        {
            return;
        }

        ChangeState();
        ActiveState();
        CompletedBattle();
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
            if (_entityList.Count == _entitiesState.Count)
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
                    _entityList = new List<(int id, CEntity entity)>();

                    int id = 0;

                    for (int i = 0; i < _heroParty.HeroList.Count; i++)
                    {
                        _entityList.Add((id, _heroParty.HeroList[i]));
                        id++;
                    }

                    for (int i = 0; i < _enemyParty.EnemyList.Count; i++)
                    {
                        _entityList.Add((id, _enemyParty.EnemyList[i]));
                        id++;
                    }

                    for (int i = 0; i < _entityList.Count; i++)
                    {
                        _entitiesState.Add(_entityList[i].entity, _combatUI.CreateStateUI(_entityList[i].entity));
                        _entitiesIndicator.Add(_entityList[i].entity, _combatUI.CreateIndicatorUI(_entityList[i].entity));
                    }
                }
                _debugMenu.Initialize();
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
        if (_actionCoroutine != null)
        {
            return;
        }

        _currentTurnState = TurnState.Normal;

        for (int i = 0; i < _entityList.Count; i++)
        {
            if (_entityList[i].entity.IsDead)
            {
                continue;
            }

            // 보스 개체는 행동 횟수를 여러번 가집니다.
            for (int actions = 0; actions < _entityList[i].entity.EntityData.Actions; actions++)
            {
                // 스피드 토큰을 가지고 있을시 행동횟수 1번 속도 +100 보정
                bool hasToken = _entityList[i].entity.TokenManager.ConsumeToken(EEffectType.Speed);

                if (hasToken)
                {
                    _messageManager.ShowMessage(_entityList[i].entity.gameObject, CCombatFloatingText.EMessageType.Speed, "속도!");
                }

                _entityList[i].entity.RoundRandomSpeed();
                _turnList.Add((_entityList[i].id, _entityList[i].entity, _entityList[i].entity.Speed + (hasToken ? 100 : 0)));
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
        if (_actionCoroutine != null)
        {
            return;
        }

        if (_turnList.Count == 0)
        {
            return;
        }

        // TODO : MY Turn Indicator
        if(MyTurnIndicator != null)
        {
            MyTurnIndicator.HideIndicator();
        }

        if (_entitiesIndicator.TryGetValue(MyTurn, out MyTurnIndicator))
        {
            MyTurnIndicator.ViewMyturnIndicator();
        }

        // 행동하기전 상태 체크
        EntityState entityState = CheckState();

        // 상태에 맞는 행동 실행
        switch (entityState)
        {
            case EntityState.Success:
                _actionCoroutine = StartCoroutine(CoAction());
                break;
            case EntityState.Dead:
                _actionCoroutine = StartCoroutine(CoDeath());
                break;
            case EntityState.Stun:
                _actionCoroutine = StartCoroutine(CoStun());
                break;
            case EntityState.Daze:
                _actionCoroutine = StartCoroutine(CoDaze());
                break;
            case EntityState.Stress:
                _actionCoroutine = StartCoroutine(CoStress());
                break;
        }
    }
    // 엔티티 상태 체크
    private EntityState CheckState()
    {
        if (_currentTurnState == TurnState.Normal)
        {
            if (_currentStressHero == null)
            {
                DotProcess();
            }
        }

        if (_currentStressHero != null)
        {
            _currentStressHero = null;
        }

        if (MyTurn.IsDead)
        {
            return EntityState.Dead;
        }

        if (MyTurn is CPlayer hero)
        {
            bool isStress = hero.CurrentStress >= 10;
            if (isStress)
            {
                _currentStressHero = hero;
                return EntityState.Stress;
            }
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
            _messageManager.ShowMessage(MyTurn.gameObject, dotType[i], dotList[i].ToString(), default, delay * 0.3f);
            delay++;
        }
    }
    // 턴이 왔는데 도트뎀으로 죽은 적들 처리
    private IEnumerator CoDeath()
    {
        yield return new WaitForSeconds(_turnSkipDelay);

        for (int i = 0; i < _entityList.Count; i++)
        {
            if (_turnList[0].id == _entityList[i].id)
            {
                bool isHero = MyTurn is CPlayer;

                if (isHero)
                {
                    RemoveDeathEntity(i);
                }
                else
                {
                    CEnemy enemy = _entityList[i].entity as CEnemy;

                    if (enemy.Corpse.IsAvailable)
                    {
                        // 시체 상태로 변환 시 한번만 턴순서 제거 처리
                        if (_corpseCheck.Contains(enemy))
                        {
                            continue;
                        }

                        RemoveCorpseEntity(i);
                        _corpseCheck.Add(enemy);
                    }
                    else if (enemy.Corpse.IsRemoved)
                    {
                        RemoveDeathEntity(i);
                    }
                }
                break;
            }
        }
        // 디버그용 드롭박스
        _debugMenu.SetDropboxEntityList();
        _combatUI.MoveTurnOrder(_turnPortraitList, true);
        yield return _sortCoroutine;
        _actionCoroutine = null;
    }
    // 엔티티끼리 상호작용하고 죽은 엔티티를 확인
    private void CheckEntitiesDeath()
    {
        bool isDirty = false;

        for (int i = _entityList.Count - 1; i >= 0; i--)
        {
            if (_entityList[i].entity.IsDead)
            {
                bool isHero = _entityList[i].entity is CPlayer;


                if (isHero)
                {
                    RemoveDeathEntity(i);
                    isDirty = true;
                }
                else
                {
                    CEnemy enemy = _entityList[i].entity as CEnemy;

                    if (enemy.Corpse.IsAvailable)
                    {
                        // 시체 상태로 변환 시 한번만 턴순서 제거 처리
                        if (_corpseCheck.Contains(enemy))
                        {
                            continue;
                        }

                        RemoveCorpseEntity(i);
                        _corpseCheck.Add(enemy);
                        isDirty = true;
                    }
                    else if (enemy.Corpse.IsRemoved)
                    {
                        RemoveDeathEntity(i);
                        isDirty = true;
                    }
                }
            }
        }
        if (isDirty)
        {
            // 디버그용 드롭박스
            _debugMenu.SetDropboxEntityList();
            _combatUI.MoveTurnOrder(_turnPortraitList, true);
        }
    }
    // 죽은 적 리스트에서 제거
    private void RemoveDeathEntity(int index)
    {
        CEntity entity = _entityList[index].entity;

        bool isHero = entity is CPlayer;

        if (isHero)
        {
            _heroParty.RemoveHero(entity as CPlayer);
        }
        else
        {
            _enemyParty.RemoveEnemy(entity as CEnemy);
        }


        // 턴 리스트와 턴순서 초상화 리스트에서 죽은 개체 확인
        for (int i = _turnList.Count - 1; i >= 0; i--)
        {
            // 동일한 엔티티인지 확인
            if (_entityList[index].id == _turnList[i].id)
            {
                _turnList.RemoveAt(i);
                Destroy(_turnPortraitList[i], _portraitRemoveDelay);
                _turnPortraitList.RemoveAt(i);
            }
        }

        Destroy(_entityList[index].entity.gameObject, _portraitRemoveDelay);
        _entitiesState.Remove(_entityList[index].entity);
        _entitiesIndicator.Remove(_entityList[index].entity);
        _entityList.RemoveAt(index);

        SortEntitiesRank();
    }
    // 시체 상태 턴 리스트 제거
    private void RemoveCorpseEntity(int index)
    {
        CEntity entity = _entityList[index].entity;

        bool isHero = entity is CPlayer;

        // 일단 시체가 적밖에 없으니..
        if (isHero)
        {
            return;
        }

        if (_entitiesState.ContainsKey(entity))
        {
            CEntityState entityState = _entitiesState[entity];
            _entitiesState.Remove(entity);
            CEnemyCorpse corpse = (entity as CEnemy).Corpse;

            entityState.ReConnect(corpse.gameObject);
            _entitiesState.Add(corpse, entityState);
        }

        for (int i = _turnList.Count - 1; i >= 0; i--)
        {
            if (_entityList[index].id == _turnList[i].id)
            {
                _turnList.RemoveAt(i);
                Destroy(_turnPortraitList[i], _portraitRemoveDelay);
                _turnPortraitList.RemoveAt(i);
            }
        }
    }
    // 기절 : 현재 라운드의 차례를 넘깁니다.
    private IEnumerator CoStun()
    {
        _messageManager.ShowMessage(MyTurn.gameObject, CCombatFloatingText.EMessageType.Stun, "기절!");
        _turnList.RemoveAt(0);
        yield return new WaitForSeconds(_actionDelay);
        _combatUI.MoveTurnOrder(_turnPortraitList);
        Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
        _turnPortraitList.RemoveAt(0);
        _actionCoroutine = null;
    }
    // 현기증 : 자신의 차례를 맨 뒤로 보냅니다. 2중첩시 기절로 변경됩니다.
    private IEnumerator CoDaze()
    {
        _messageManager.ShowMessage(_turnList[0].entity.gameObject, CCombatFloatingText.EMessageType.Daze, "현기증!");

        _turnList.Add(_turnList[0]);
        _turnList.RemoveAt(0);
        yield return new WaitForSeconds(_turnSkipDelay);
        _turnPortraitList.Add(_combatUI.CopyTurnOrderPortrait(_turnPortraitList[0]));
        _combatUI.MoveTurnOrder(_turnPortraitList);
        _turnPortraitList.RemoveAt(0);
        _actionCoroutine = null;
    }
    // 스트레스
    private IEnumerator CoStress()
    {
        _actionCoroutine = StartCoroutine(_combatCameraManager.CoHeroTemptation(MyTurn as CPlayer));
        yield return _actionCoroutine;
        _actionCoroutine = null;
    }
    // 행동
    private IEnumerator CoAction()
    {
        bool doAction = false;
        bool isHero = MyTurn is CPlayer;

        OpenTurnSetting();

        yield return _tryBeginCoroutine;

        MyTurn.OnActionRequested += OnActionData;

        // Could : 턴 시작 시 추가 행동 악세사리 확인

        // 액션할때까지 기다리기  
        while (!doAction)
        {
            if (isHero)
            {

                if (_combatUI.Pass)
                {
                    doAction = true;
                    _messageManager.ShowMessage(MyTurn.gameObject, CCombatFloatingText.EMessageType.None, "턴넘김");
                    _combatUI.Pass = false;
                    break;
                }

                // TODO : 자리 변경 구현필요
                if (_combatUI.Change)
                {
                    doAction = true;
                    break;
                }

                // 전투 화면연출
                doAction = _doingAction;

                if (_doingAction)
                {
                    _combatUI.CloseHeroInfomation();
                    MyTurn.TryStartRequestedAction();
                    SnapMoveSkill();

                    _combatUI.UseSkill(_currentActionSkillData);
                    IReadOnlyList<CEntity> targets = ((CPlayer)MyTurn).ActionTargets;
                    _combatCoroutine = _combatCameraManager.CombatAction(MyTurn, targets);
                    yield return _combatCoroutine;

                    _combatCoroutine = null;
                    _doingAction = false;
                }
            }
            else
            {
                CEnemyAI actor = MyTurn.GetComponent<CEnemyAI>();
                actor.TryRequestPlannedAction();
                EEnemyActionRequestResult result = actor.RequestPlannedAction();
                CEnemy enemy = MyTurn as CEnemy;
                bool isBusy = enemy.IsPresentationBusy;
                EEntityActionState actionState = enemy.ActionState;

                switch (result)
                {
                    case EEnemyActionRequestResult.Rejected:
                        doAction = _doingAction;

                        if (_doingAction)
                        {
                            yield return new WaitForSeconds(_actionDelay);
                            MyTurn.TryStartRequestedAction();
                            SnapMoveSkill();

                            _combatUI.UseSkill(_currentActionSkillData);
                            IReadOnlyList<CEntity> targets = actor.PlannedTargets;
                            _combatCoroutine = _combatCameraManager.CombatAction(MyTurn, targets);
                            yield return _combatCoroutine;

                            yield return new WaitUntil(() => enemy.ActionState == EEntityActionState.Idle);

                            _combatCoroutine = null;
                            _doingAction = false;
                        }
                        break;
                    case EEnemyActionRequestResult.Requested:
                        break;
                    case EEnemyActionRequestResult.WaitingForPresentation:
                        break;
                    case EEnemyActionRequestResult.NoAvailableAction:
                        doAction = true;
                        break;
                }
            }
            yield return null;
        }

        MyTurn.OnActionRequested -= OnActionData;

        SortEntitiesPosition();
        yield return _sortCoroutine;

        yield return new WaitForSeconds(_actionDelay);

        _combatUI.CloseCombatPanel();
        _combatUI.CloseHeroInfomation();
        _combatUI.CloseEnemyInfomation();

        bool _isAfterDead = MyTurn.IsDead;

        if (!_isAfterDead)
        {
            // Could :추가 턴 악세사리 여부 체크

            // 턴 상태 체크
            CheckTurnState();
        }

        CheckEntitiesDeath();
        yield return _sortCoroutine;

        // Check HeroList Stress
        for (int i = 0; i < _heroParty.HeroList.Count; i++)
        {
            CPlayer hero = _heroParty.HeroList[i];

            if (hero.CurrentStress >= 10)
            {
                CPrint.Log(hero.HeroData.Name);
                _multiStressCoroutine = StartCoroutine(_combatCameraManager.CoHeroTemptation(hero));

                yield return _multiStressCoroutine;

                _multiStressCoroutine = null;
            }
        }
        _actionCoroutine = null;
    }
    // 현재 사용할 스킬 데이터
    private void OnActionData(CEntity actor, CEntity target, CSkillDataSO skill)
    {
        CPrint.Log(actor.name + " -> " + target.name + " / " + skill.SkillName);
        _currentActionSkillData = skill;
        _doingAction = true;
    }
    // 이동 스킬 사용 시 캐릭터 이동 적용
    private void SnapMoveSkill()
    {
        for (int i = 0; i < _entityList.Count; i++)
        {
            bool isHero = _entityList[i].entity is CPlayer;
            bool isEnemy = _entityList[i].entity is CEnemy;
            CEntity entity = _entityList[i].entity;

            if (isHero)
            {
                CPlayer hero = entity as CPlayer;
                hero.RankPresentation.SnapToRank();
            }
            if (isEnemy)
            {
                CEnemy enemy = entity as CEnemy;
                enemy.FormationMovement.SnapToRank();
            }
        }
    }
    // 현재 턴 행동시 UI 오픈
    private void OpenTurnSetting()
    {
        _combatUI.CloseHeroInfomation();
        _combatUI.CloseEnemyInfomation();

        bool isHero = MyTurn is CPlayer;

        if (isHero)
        {
            _heroBattleInput.SetHero(MyTurn as CPlayer);
            _heroBattleInput.SetTargets(_enemyParty.EnemyList, Vector3.zero);
            _heroBattleInput.AddAllyTargets(_heroParty.HeroList, Vector3.zero);
            _combatUI.OpenHeroInfomation((CPlayer)MyTurn, _heroBattleInput);
            MyTurn.TryBeginAction();
            CPlayer hero = MyTurn as CPlayer;
        }
        // 적의 턴인 경우
        else
        {
            CEnemy enemy = MyTurn as CEnemy;
            _combatUI.OpenEnemyInfomation(enemy, false);

            CEnemyAI actor = enemy.GetComponent<CEnemyAI>();

            if (actor == null)
            {
                actor = enemy.gameObject.AddComponent<CEnemyAI>();
            }

            actor.TryPlanAction(_heroParty.HeroList, _enemyParty.EnemyList);
            bool beginAction = enemy.TryBeginAction();

            if (!beginAction)
            {
                _tryBeginCoroutine = StartCoroutine(CoTryBeginAction(enemy));
            }
        }
    }
    // 적이 액션할 준비가 되었는지 체크
    private IEnumerator CoTryBeginAction(CEnemy enemy)
    {
        bool beginAction = enemy.TryBeginAction();

        while (!beginAction)
        {
            beginAction = enemy.TryBeginAction();
            yield return null;
        }
    }
    // 추가 행동 체크
    private void CheckTurnState()
    {
        if (MyTurn.IsDead)
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
    // 엔티티 랭크 갱신이 필요한 상황
    private void SortEntitiesRank()
    {
        CPrint.Warn($"엔티티 리스트 제거로 인한 포지션 정리 Hero : [{_heroParty.HeroList.Count}] / Enemy : [{_enemyParty.EnemyList.Count}] ");
        // 1. hero
        _heroParty.SortRank();
        // 2. enemy
        _enemyParty.SortRank();

        SortEntitiesPosition(true);
    }
    // 변경된 엔티티 위치 재조정
    private void SortEntitiesPosition(bool forceCleanup = false)
    {
        List<Transform> oldTransforms = new List<Transform>();
        List<Vector3> initPosList = new List<Vector3>();
        List<Vector3> desirePosList = new List<Vector3>();


        // 랭크가 제대로 배치되어 있는지 체크
        CPrint.Log("Rank 유효성 검사중...");
        bool isHeroRankValid = _heroParty.CheckRankValid();

        if (!isHeroRankValid)
        {
            _heroParty.SortRank();
        }

        bool isEnemyRankVaild = _enemyParty.CheckRankValid();

        if (!isEnemyRankVaild)
        {
            _enemyParty.SortRank();
        }

        // 이동할 영웅 개체 찾기
        CPrint.Log("Hero 정리목록 확인중...");
        for (int i = 0; i < _heroParty.HeroList.Count; i++)
        {
            Vector3 desirePos = Vector3.zero;

            CPlayer hero = _heroParty.HeroList[i];
            ERank rank = hero.CurrentRank;

            bool hasRank = _heroParty.RankList.TryGetValue(rank, out CPlayer prevHero);

            if (!hasRank)
            {
                CPrint.Warn($"Dictionary에 [{rank}] 존재하지않음. 정리 필요");
            }
            else
            {
                ERank prevRank = prevHero.CurrentRank;

                // 랭크가 변화가 없는가?
                if (!forceCleanup && hero == prevHero)
                {
                    continue;
                }
            }

            CPrint.Log($"정리대상 : [{hero.HeroData.Name}]");
            int rankIdx = (int)System.MathF.Log((float)rank, 2);

            Transform oldTr = hero.gameObject.transform;
            Vector3 initPos = oldTr.position;
            desirePos = HeroRankPosition[rankIdx].transform.position;

            oldTransforms.Add(oldTr);
            initPosList.Add(initPos);
            desirePosList.Add(desirePos);
        }

        // 이동할 적 개체 찾기
        CPrint.Log("Enemy 정리목록 확인중...");

        for (int i = 0; i < _enemyParty.EnemyList.Count; i++)
        {
            Vector3 desirePos = Vector3.zero;

            CEnemy enemy = _enemyParty.EnemyList[i];
            ERank rank = enemy.CurrentRank;

            bool isValid = _enemyParty.RankList.TryGetValue(rank, out CEnemy prevEnemy);

            if (!isValid)
            {
                CPrint.Warn($"Dictionary에 [{rank}] 존재하지않음. 정리 필요.");
            }
            else
            {
                // 랭크 변화 없음
                if (!forceCleanup && enemy == prevEnemy)
                {
                    continue;
                }
            }

            CPrint.Log($"정리대상 : [{enemy.EnemyData.Name}] / Corpse : ({enemy.Corpse.IsAvailable})");
            int rankIdx = (int)System.MathF.Log((float)rank, 2);

            Transform oldTr = enemy.gameObject.transform;
            Vector3 initPos = oldTr.position;

            int size = enemy.EnemyData.FormationSize;

            if (size <= 0 || rankIdx < 0 || rankIdx + size > 4)
            {
                CPrint.Error($"잘못된 진형 데이터: {enemy.EnemyData.Name}");
                continue;
            }

            for (int j = rankIdx; j < rankIdx + size; j++)
            {
                desirePos += EnemyRankPosition[j].transform.position;
            }

            desirePos = new Vector3(desirePos.x / size, desirePos.y / size, desirePos.z / size);

            oldTransforms.Add(oldTr);
            initPosList.Add(initPos);
            desirePosList.Add(desirePos);

        }

        CPrint.Log($"Entity 정리대상 {oldTransforms.Count} 개체");

        if (oldTransforms.Count == 0)
        {
            return;
        }

        _sortCoroutine = StartCoroutine(CoEntitiesPositionMove(oldTransforms, initPosList, desirePosList));
    }
    private IEnumerator CoEntitiesPositionMove(List<Transform> oldTransforms, List<Vector3> initPosList, List<Vector3> desirePosList)
    {
        float delay = 1f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            for (int i = 0; i < oldTransforms.Count; i++)
            {
                if (oldTransforms[i] == null)
                {
                    continue;
                }

                float t = 1f - Mathf.Exp(-5 * Mathf.Clamp01(1f - (nextTime - Time.time) / delay));

                oldTransforms[i].position = Vector3.Lerp(initPosList[i], desirePosList[i], t);
            }
            yield return null;
        }

        _sortCoroutine = null;
    }

    // 전투가 완료되었는지 확인
    private void CompletedBattle()
    {
        if (true)
        {
            return;
        }

        if (!CHeroParty.IsReady)
        {
            return;
        }

        if (!CEnemyParty.IsReady)
        {
            return;
        }

        if (_battleEnd)
        {
            return;
        }

        bool victory = false;

        if (_enemyParty.EnemyList.Count == 0)
        {
            victory = true;
        }
        else
        {
            bool notAlive = true;

            for (int i = 0; i < _enemyParty.EnemyList.Count; i++)
            {
                CEnemy enemy = _enemyParty.EnemyList[i];

                if (!enemy.IsDead)
                {
                    notAlive = false;
                }
            }

            if (notAlive)
            {
                victory = true;
            }

        }

        if (victory)
        {
            _heroParty.HeroExpeditionState.TryCompleteBattle(_heroParty.RunID, _heroParty.BattleID, _heroParty.HeroList);
            CPrint.Warn("전투 승리!");
            _battleEnd = true;
            return;
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