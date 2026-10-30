using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CBasicCombatManager : MonoBehaviour
{
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

    public enum EntityState
    {
        Success,
        Dead,
        Stun,
        Daze,
    }


    #region Inspector
    [Header("Draw Canvas")]
    [SerializeField] private Canvas _canvas;

    [Header("HeroParty")]
    [SerializeField] private CHeroParty _heroParty;

    [Header("MessageManager")]
    [SerializeField] private CCombatFloatingMessageManager _messageManager;

    [Header("Debug Menu")]
    [SerializeField] private CCombatDebugMenu _debugMenu;

    [Header("CombatManager State")]
    // 현재 필드에 서있는 엔티티 리스트
    [SerializeField] private List<CEntity> _entityList;
    // 현재 라운드 상태
    [SerializeField] private RoundState _roundState = RoundState.None;

    [Header("Hero Infomation")]
    // 영웅 정보 패널
    [SerializeField] private GameObject _heroInfo;
    [SerializeField] private GameObject _heroPortrait;
    [SerializeField] private TextMeshProUGUI _heroName;

    [SerializeField] private GameObject _heroSkillPanel;
    [SerializeField] private TextMeshProUGUI _heroSkillMainDesc;
    [SerializeField] private TextMeshProUGUI _heroSkillSubDesc;

    [Header("Skill Panel")]
    // 영웅 스킬 항목
    [SerializeField] private GameObject _skillListPanel;
    // 현재 동작중인 스킬 이름
    [SerializeField] private TextMeshProUGUI _currentSkillName;
    // 스킬 패널
    [SerializeField] private GameObject _cursorSkillInfo;
    [SerializeField] private GameObject _cursorSkill;
    [SerializeField] private TextMeshProUGUI _cursorSkillName;
    // 현재 스킬의 적과 아군의 랭크 표시
    [SerializeField] private GameObject _AlliesRankPanel;
    [SerializeField] private List<Image> _AlliesRank;
    [SerializeField] private GameObject _targetRankPanel;
    [SerializeField] private List<Image> _targetRank;
    // 5개 슬롯과 스킬
    [SerializeField] private List<GameObject> _skillSlot;
    [SerializeField] private List<Image> _skillImage;
    // 전투 아이템
    [SerializeField] private GameObject _combatItemSlot;
    [SerializeField] private Image _combatItemImage;
    // 자리 변경
    [SerializeField] private GameObject _rankChangeBtn;
    // 한턴 쉬기
    [SerializeField] private GameObject _passBtn;

    [Header("Enemy Infomation")]
    [SerializeField] private GameObject _enemyInfo;
    [SerializeField] private GameObject _enemyPortrait;
    [SerializeField] private TextMeshProUGUI _enemyName;
    // 적 상세항목 패널
    [SerializeField] private GameObject _enemyStatus;
    [SerializeField] private TextMeshProUGUI _enemyType;
    // HP
    [SerializeField] private TextMeshProUGUI _enemyMaxHealth;
    [SerializeField] private TextMeshProUGUI _enemyCurHealth;
    [SerializeField] private Image _enemyHpBarFill;
    // Status
    [SerializeField] private TextMeshProUGUI _enemySpeed;
    [SerializeField] private TextMeshProUGUI _enemyBleedResist;
    [SerializeField] private TextMeshProUGUI _enemyBlightResist;
    [SerializeField] private TextMeshProUGUI _enemyBurnResist;
    [SerializeField] private TextMeshProUGUI _enemyStunResist;
    [SerializeField] private TextMeshProUGUI _enemyMoveResist;
    [SerializeField] private TextMeshProUGUI _enemyWeakResist;

    [Header("Entity State Prefab")]
    [SerializeField] private GameObject _entityStateRoot;
    [SerializeField] private GameObject _entityStatePrefab;

    // 턴 초상화 이미지 프리팹과 보관 위치
    [Header("Turnorder Prefab")]
    [SerializeField] private GameObject _turnOrderRoot;
    [SerializeField] private GameObject _turnOrderPrefab;

    [Header("Round Text")]
    [SerializeField] private TextMeshProUGUI _roundText;

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
    private GameObject _myTurn = null;
    private Coroutine _coroutine = null;
    private int _round = 0;
    private TurnState _currentTurnState = TurnState.Normal;
    // 턴 넘김 확인
    private bool _pass = false;
    // 행동 완료 알림
    private bool _actionCompleted = false;
    #endregion

    #region Property
    public List<CEntity> EntityList => _entityList;
    public Dictionary<CEntity, CEntityState> EntityStateList => _entityStateList;
    #endregion

    private void Awake()
    {
        // TODO : 널체크  


        #region 리스너 등록
        for (int i = 0; i < _skillSlot.Count; i++)
        {
            int idx = i;
            _skillSlot[i].GetComponent<Button>().onClick.AddListener(() => OnSelectSkill(idx));
        }

        _passBtn.GetComponent<Button>().onClick.AddListener(OnActionPass);
        #endregion
    }
    // 턴넘김
    private void OnActionPass()
    {
        _pass = true;
    }
    private void OnSelectSkill(int idx)
    {
        if (!_skillListPanel.activeSelf)
        {
            return;
        }

        if (_turnList[0].entity is not CPlayer)
        {
            return;
        }

        RectTransform SkillSlot = _skillSlot[idx].GetComponent<RectTransform>();
        RectTransform cursorRectTr = _cursorSkill.GetComponent<RectTransform>();
        cursorRectTr.anchoredPosition = SkillSlot.anchoredPosition;

        CPlayer hero = _turnList[0].entity as CPlayer;

        CSkillDataSO skillSO = hero.EquippedSkills[idx];

        _cursorSkillInfo.SetActive(true);
        _cursorSkillName.text = skillSO.SkillName;

        _cursorSkill.SetActive(true);

        // TODO : 해당 영웅의 스킬 사용모션 동작
        hero.SelectSkill(idx);

    }

    private void Start()
    {
        if (CInputDispatcher.Instance != null)
        {
            // 스킬 사용 키 등록
            CInputDispatcher.Instance.OnSkill += OnSelectSkill;
            CPrint.Log("디스패쳐와 연결 완료");
        }
    }



    private void Update()
    {
        ChangeState();
        ActiveState();
    }

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
            if(_entityList.Count < _entityStateList.Count)
            {
                CPrint.Error("상태창이 더 많이 생성되었습니다. 체크 필요");
                enabled = false;
                return;
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
    private void ActiveState()
    {
        switch (_roundState)
        {
            case RoundState.Wait:
                if (CHeroParty.IsReady)
                {
                    for (int i = 0; i < _heroParty.HeroList.Count; i++)
                    {
                        _entityList.Add(_heroParty.HeroList[i]);
                        // →← TODO : 추후 구독 제거작업 필수!
                        _heroParty.HeroList[i].OnActionCompleted += DoAction;
                    }

                    for (int i = 0; i < _entityList.Count; i++)
                    {
                        // 엔티티 상태창 추가
                        CEntityState entityState = Instantiate(_entityStatePrefab, _entityStateRoot.transform).GetComponent<CEntityState>();
                        entityState.Initialize(_canvas, _entityList[i].gameObject);
                        _entityStateList.Add(_entityList[i], entityState);
                    }

                    _debugMenu.Initialize();
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
                _entityList[i].RoundRandomSpeed();
                _turnList.Add((_entityList[i], _entityList[i].Speed));
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
        for (int i = 0; i < _turnList.Count; i++)
        {
            GameObject obj = Instantiate(_turnOrderPrefab, _turnOrderRoot.transform);

            CPortraitImage portraitImage = obj.GetComponent<CPortraitImage>();

            Sprite portrait = _turnList[i].entity.EntityData.PortraitTurnOrder;
            // 초상화 마커 그리기
            bool isHero = _turnList[i].entity is CPlayer;
            EEntityType entityType = isHero ? EEntityType.Hero : EEntityType.Enemy;

            // 4 3 2 1 초상화 배치 및 이동
            if (i < 4)
            {
                portraitImage.Initialize(4, entityType, portrait);
                portraitImage.Move(i + 1);
            }
            else
            {
                portraitImage.Initialize(i + 1, entityType, portrait);
            }

            _turnPortraitList.Add(obj);
        }

        // Debug
        for (int i = 0; i < _turnList.Count; i++)
        {
            CPrint.Log($"index[{i}] :  [{_turnList[i].entity.name} / {_turnList[i].turnSpeed}]");
        }

        // 라운드 올리기
        NextRound();
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

        if (_myTurn != null)
        {
            Destroy(_myTurn);
        }

        _myTurn = _messageManager.ShowIcon(_turnList[0].entity.gameObject, CCombatFloatingIcon.IconType.Turn);

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

        if (_turnList[0].entity.IsDead)
        {
            return EntityState.Dead;
        }

        // TODO : 스트레스 받았을 시 연출 추가
        bool isMeltDown = false;

        if (isMeltDown)
        {

        }

        bool isStuned = _turnList[0].entity.TokenManager.ConsumeToken(EEffectType.Stun);

        if (isStuned)
        {
            //ContainToken(_turnList[0].entity, EEffectType.Stun);
            return EntityState.Stun;
        }

        bool isDazed = _turnList[0].entity.TokenManager.ConsumeToken(EEffectType.Daze);

        if (isDazed)
        {
            //ContainToken(_turnList[0].entity, EEffectType.Daze);
            return EntityState.Daze;
        }

        return EntityState.Success;
    }
    private void DotProcess()
    {
        // DOT 데미지 들어오는거 체크 후 죽음 상태로 변경
        bool ishero = _turnList[0].entity is CPlayer;

        (int bleed, int blight, int burn) total = (0, 0, 0);

        if (ishero)
        {
            CPlayer hero = _turnList[0].entity as CPlayer;
            // 현재 턴에 들어올 데미지 가져오기
            total = (hero.GetTotalDotDamageByType(EEffectType.Bleed), hero.GetTotalDotDamageByType(EEffectType.Blight), hero.GetTotalDotDamageByType(EEffectType.Burn));
            // 도트 데미지 적용
            hero.OnTurnStart();
        }
        else
        {
            CEnemy enemy = _turnList[0].entity as CEnemy;
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

        // 체력 및 도트 남았는지 후처리
        //_entityStateList[_turnList[0].entity].SetHealth();
        //ContainDot(_turnList[0].entity, ishero);
    }
    // 도트 처리 후 아직 남았는가?
    private void ContainDot(CEntity entity, bool isHero)
    {
        List<EEffectType> dotList = new List<EEffectType>() { EEffectType.Bleed, EEffectType.Blight, EEffectType.Burn };

        if (isHero)
        {
            CPlayer hero = entity as CPlayer;

            for (int i = 0; i < dotList.Count; i++)
            {
                if (hero.GetTotalDotDamageByType(dotList[i]) <= 0)
                {
                    _entityStateList[entity].SetEffect(dotList[i], false);
                }
            }
        }
        else
        {
            CEnemy enemy = entity as CEnemy;

            for (int i = 0; i < dotList.Count; i++)
            {
                if (enemy.GetTotalDotDamageByType(dotList[i]) <= 0)
                {
                    _entityStateList[entity].SetEffect(dotList[i], false);
                }
            }
        }
    }
    // 토큰 체크
    private void ContainToken(CEntity entity, EEffectType effectType)
    {
        if (!entity.TokenManager.HasToken(effectType))
        {
            _entityStateList[entity].SetEffect(effectType, false);
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
        MoveTurnOrder(true);
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
            MoveTurnOrder(true);
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
        MoveTurnOrder();
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
        GameObject copyObj = Instantiate(_turnPortraitList[0], _turnOrderRoot.transform);
        _turnPortraitList.Add(copyObj);
        MoveTurnOrder();
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

        Vector3 mousePos;
        Ray ray;
        _cursorSkillInfo.SetActive(false);
        _cursorSkill.SetActive(false);
        // 턴 시작 시 추가 행동 악세사리 확인
        //CheckAddExtraTurn(CTempEntity.EJewelryType.TurnStart);

        // 액션할때까지 기다리기  
        while (!doAction)
        {
            if (_pass)
            {
                break;
            }

            // 현재 행동턴이 적인지 아군인지 구별하여 인터페이스를 동작시켜야 합니다.
            // 해당 코드는 완성에만 목표로 작성된 문법이니 최적화가 필요합니다.
            bool isHero = _turnList[0].entity is CPlayer;

            // 히어로일 때
            // 1.적에게 마우스를 올렸을 때 정보가 나와야 합니다.
            // 2.현재 턴인 히어로의 정보가 나와야합니다.
            if (isHero)
            {
                CPlayer hero = _turnList[0].entity as CPlayer;
                CHeroDataSO heroSO = (CHeroDataSO)hero.EntityData;

                // 플레이어 UI [ TODO : 추후 한번만 변경되도록 구현해야합니다. ]
                _heroInfo.SetActive(true);
                _skillListPanel.SetActive(true);

                for (int i = 0; i < _skillSlot.Count; i++)
                {
                    _skillSlot[i].GetComponent<Image>().sprite = heroSO.SkillSlotBackground;
                    _skillImage[i].sprite = hero.EquippedSkills[i].SkillIcon;
                }

                _heroName.text = heroSO.DefaultName;
                _heroPortrait.GetComponent<Image>().sprite = heroSO.PortraitInfo;

                // 적 UI
                mousePos = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(mousePos);

                Physics.Raycast(ray, out RaycastHit hitInfo);

                if (hitInfo.collider != null)
                {
                    // 임시 : 현재 TempEntity라서 추후에 뜯어고쳐야합니다.
                    bool isEnemy = hitInfo.collider.gameObject.TryGetComponent<CEnemy>(out CEnemy hitEntity);

                    if (isEnemy)
                    {
                        CEnemyDataSO enemyDataSO = hitEntity.EnemyData;

                        _enemyName.text = enemyDataSO.Name;
                        _enemyPortrait.GetComponent<Image>().sprite = enemyDataSO.PortraitInfo;
                        _enemyMaxHealth.text = enemyDataSO.ToString();
                        _enemyCurHealth.text = hitEntity.CurrentHp.ToString();
                        _enemyHpBarFill.fillAmount = Mathf.Clamp01((float)hitEntity.CurrentHp / (float)enemyDataSO.MaxHealth);

                        _enemySpeed.text = enemyDataSO.ToString();
                        _enemyBleedResist.text = enemyDataSO.Resistance.Bleed.ToString();
                        _enemyBlightResist.text = enemyDataSO.Resistance.Blight.ToString();
                        _enemyBurnResist.text = enemyDataSO.Resistance.Burn.ToString();
                        _enemyStunResist.text = enemyDataSO.Resistance.Stun.ToString();
                        _enemyMoveResist.text = enemyDataSO.Resistance.Move.ToString();
                        _enemyWeakResist.text = enemyDataSO.Resistance.Debuff.ToString();

                        _enemyInfo.SetActive(true);
                        _enemyStatus.SetActive(true);
                    }
                    else
                    {
                        _enemyInfo.SetActive(false);
                        _enemyStatus.SetActive(false);
                    }
                }
                else
                {
                    _enemyInfo.SetActive(false);
                    _enemyStatus.SetActive(false);
                }
            }
            // 적의 턴인 경우
            else
            {
                _heroInfo.SetActive(false);
                _skillListPanel.SetActive(false);

                CEnemy enemy = _turnList[0].entity as CEnemy;
                CEnemyDataSO enemyDataSO = enemy.EnemyData;

                // 이름 반영
                _enemyName.text = enemyDataSO.Name;
                // 초상화 반영
                _enemyPortrait.GetComponent<Image>().sprite = enemyDataSO.PortraitInfo;

                // 체력 반영
                _enemyMaxHealth.text = enemyDataSO.MaxHealth.ToString();
                _enemyCurHealth.text = enemy.CurrentHp.ToString();
                _enemyHpBarFill.fillAmount = Mathf.Clamp01((float)enemy.CurrentHp / (float)enemyDataSO.MaxHealth);


                _enemyInfo.SetActive(true);
            }
            // →← TODO : 행동을 완료하였을 때 처리

            doAction = _actionCompleted;
            yield return null;
        }

        _messageManager.ShowMessage(_turnList[0].entity.gameObject, CCombatFloatingText.EMessageType.None, !_pass ? "행동" : "턴넘김");
        _pass = false;
        _actionCompleted = false;

        yield return new WaitForSeconds(_actionDelay);

        bool _isAfterDead = _turnList[0].entity.IsDead;

        if (!_isAfterDead)
        {
            //CheckAddExtraTurn(CTempEntity.EJewelryType.TurnEnd);
            CheckTurnState();
        }

        CheckEntitiesDeath();
        _coroutine = null;
    }

    private void DoAction(CEntity entity)
    {
        _actionCompleted = true;
    }

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

    #region 추가 행동 체크
    /*
    ▶ 추가 행동 체크

    현재 확인한 방식은 

    스킬을 사용 시 추가 행동 획득 [ 적 : 사서 / 아군 : 나병환자 스킬 ]
    
    악세사리 [ 턴 시작 혹은 종료 시 / 죽음의 일격 혹은 피격 시 등등..]
    */
    #endregion
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
                MoveTurnOrder();
                Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
                _turnPortraitList.RemoveAt(0);
            }
        }
        // 추가행동 후 턴에선 
        else if (_currentTurnState == TurnState.Extra)
        {
            _turnList.RemoveAt(0);
            MoveTurnOrder();
            Destroy(_turnPortraitList[0], _turnPortraitList[0].GetComponent<CPortraitImage>().MoveTime + 0.5f);
            _turnPortraitList.RemoveAt(0);
            _currentTurnState = TurnState.Normal;
        }
    }
    private void MoveTurnOrder(bool isDirty = false)
    {
        // 죽는 상태에선 단순 하나의 요소만 변경되는 것이 아니기 때문에 리스트와 싱크를 맞춰야한다.
        if (isDirty)
        {
            for (int i = 0; i < _turnPortraitList.Count; i++)
            {
                if (_turnPortraitList.Count - 1 < i)
                {
                    break;
                }

                CPortraitImage portraitImage = _turnPortraitList[i].GetComponent<CPortraitImage>();

                portraitImage.Move(i + 1);
            }
            return;
        }


        for (int i = 0; i < _turnPortraitList.Count; i++)
        {
            if (_turnPortraitList.Count - 1 < i)
            {
                break;
            }

            CPortraitImage portraitImage = _turnPortraitList[i].GetComponent<CPortraitImage>();

            portraitImage.Move(i);
        }
        CPortraitImage.IsDirty = false;
    }
    private void NextRound()
    {
        if (_turnList.Count == 0)
        {
            return;
        }

        _round++;
        _roundText.text = _round.ToString();
    }
}
