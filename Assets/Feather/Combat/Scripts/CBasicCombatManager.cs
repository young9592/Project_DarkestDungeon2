using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CBasicCombatManager : MonoBehaviour
{
    public enum RoundState
    {
        None,
        Ready,
        Start
    }

    // 추가턴인지 아닌지 체크
    public enum TurnState
    {
        Normal,
        Extra
    }

    public enum EntityState
    {
        Success,
        Dead,
        Stun,
        Daze,
    }

    [System.Serializable]
    public struct SpriteData
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite sprite;

        public string GetID => id;
        public Sprite GetSprite => sprite;
    }

    #region Inspector
    [Header("Draw Canvas")]
    [SerializeField] private Canvas _canvas;

    [Header("CombatManager State")]
    // 현재 필드에 서있는 엔티티 리스트
    [SerializeField] private List<CTempEntity> _fieldEntitiesList;
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

    [Header("State Text Prefab")]
    [SerializeField] private GameObject _stateTextRoot;
    [SerializeField] private GameObject _stateTextPrefab;

    [Header("Icon Prefab")]
    [SerializeField] private GameObject _iconPrefabRoot;
    [SerializeField] private GameObject _deathStampPrefab;
    [SerializeField] private GameObject _stressPrefab;
    [SerializeField] private GameObject _virtuePrefab;
    [SerializeField] private GameObject _targetPrefab;

    [Header("Round Text")]
    [SerializeField] private TextMeshProUGUI _roundText;

    [Header("Option")]
    [SerializeField] private float _actionDelay = 3.5f;
    [SerializeField] private float _portraitRemoveDelay = 0.05f;
    // TODO : 전투 진입 시 엔티티들의 위치
    [SerializeField] private Vector3 _fieldEntitiesPos;

    [Header("Debug Menu")]
    [SerializeField] private GameObject _debugRoot;
    [SerializeField] private TMP_Dropdown _selectEntity;
    [SerializeField] private TMP_Dropdown _selectEffect;
    [SerializeField] private Button _shootEffectBtn;

    [Header("Test Effect")]
    [SerializeField] private CEffectSO _bleed;
    [SerializeField] private CEffectSO _blight;
    [SerializeField] private CEffectSO _burn;
    [SerializeField] private CEffectSO _daze;
    [SerializeField] private CEffectSO _stun;

    [Header("Temp Sprite input")]
    [SerializeField] private List<SpriteData> _slotList;
    [SerializeField] private List<SpriteData> _heroProfilePortraitList;
    [SerializeField] private List<SpriteData> _enemyProfilePortraitList;
    #endregion

    #region Field
    // 상태 리스트
    private Dictionary<CTempEntity, CEntityState> _entityStateList = new Dictionary<CTempEntity, CEntityState>();
    // 순서 리스트
    private List<(CTempEntity entity, int turnSpeed)> _turnList = new List<(CTempEntity, int)>();
    // 초상화 리스트
    private List<GameObject> _turnPortraitList = new List<GameObject>();
    private GameObject _myTurn = null;
    private Coroutine _coroutine = null;
    private int _round = 0;
    private TurnState _currentTurnState = TurnState.Normal;
    // 턴 넘김 확인
    private bool _pass;

    // Debug Menu Value
    private List<string> _effectOptions = new List<string> { "데미지", "출혈", "중독", "화상", "현기증", "기절", "추가행동" };
    private int _selecedtEffectIdx = 0;
    private int _selectedEntitytIdx = 0;

    // temp store Image Dictionary
    private Dictionary<string, Sprite> _slotBGDict;
    private Dictionary<string, Sprite> _heroProfilePortraitDict;
    private Dictionary<string, Sprite> _enemyProfilePortraitDict;
    #endregion

    private void Awake()
    {
        // TODO : 널체크  

        #region 임시 딕셔너리 TODO : entitySO에 프로필초상화, heroSO에 스킬슬롯 배경 이미지 추가 필요
        // TODO : 임시 데이터 저장, 추후 영웅과 적 데이터 삽입 필요
        _slotBGDict = new Dictionary<string, Sprite>();
        _heroProfilePortraitDict = new Dictionary<string, Sprite>();
        _enemyProfilePortraitDict = new Dictionary<string, Sprite>();

        for (int i = 0; i < _slotList.Count; i++)
        {
            _slotBGDict.Add(_slotList[i].GetID, _slotList[i].GetSprite);
        }

        for (int i = 0; i < _heroProfilePortraitList.Count; i++)
        {
            _heroProfilePortraitDict.Add(_heroProfilePortraitList[i].GetID, _heroProfilePortraitList[i].GetSprite);
        }

        for (int i = 0; i < _enemyProfilePortraitList.Count; i++)
        {
            _enemyProfilePortraitDict.Add(_enemyProfilePortraitList[i].GetID, _enemyProfilePortraitList[i].GetSprite);
        }
        #endregion

        #region 리스너 등록
        for (int i = 0; i < _skillSlot.Count; i++)
        {
            // 캡쳐 주의
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

        RectTransform SkillSlot = _skillSlot[idx].GetComponent<RectTransform>();
        RectTransform cursorRectTr = _cursorSkill.GetComponent<RectTransform>();
        cursorRectTr.anchoredPosition = SkillSlot.anchoredPosition;

        CSkillDataSO skillSO = _turnList[0].entity.SkillList[idx];

        _cursorSkillInfo.SetActive(true);
        _cursorSkillName.text = skillSO.SkillName;
        

        _cursorSkill.SetActive(true);
        // TODO : 해당 영웅의 스킬 사용모션 동작
    }

    private void Start()
    {
        #region Debug Menu Create
        _selectEffect.ClearOptions();

        _selectEffect.AddOptions(_effectOptions);
        _selectEffect.RefreshShownValue();

        SetDropboxEntityList();
        #endregion

        // 엔티티 상태창 생성
        for (int i = 0; i < _fieldEntitiesList.Count; i++)
        {
            CEntityState entityState = Instantiate(_entityStatePrefab, _entityStateRoot.transform).GetComponent<CEntityState>();
            entityState.Initialize(_canvas, _fieldEntitiesList[i].gameObject);
            _entityStateList.Add(_fieldEntitiesList[i], entityState);
        }

        if (CInputDispatcher.Instance != null)
        {
            // 스킬 사용 키 등록
            CInputDispatcher.Instance.OnSkill += OnSelectSkill;
            CPrint.Log("디스패쳐와 연결 완료");
        }
    }

    private void OnEnable()
    {
        _selectEffect.onValueChanged.AddListener(index => OnDropboxChanged("Effect", index));
        _selectEntity.onValueChanged.AddListener(index => OnDropboxChanged("Entity", index));
        _shootEffectBtn.onClick.AddListener(OnEffectApplyButton);
    }

    private void OnDisable()
    {
        _selectEffect.onValueChanged.RemoveAllListeners();
        _selectEntity.onValueChanged.RemoveAllListeners();
        _shootEffectBtn.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        // Debug Menu Key
        DebugMenuKey();

        ChangeState();
        ActiveState();
    }
    #region Debug Option Method
    private void DebugMenuKey()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            _debugRoot.SetActive(!_debugRoot.activeSelf);
        }
    }
    // 디버그용 엔티티 드롭박스
    private void SetDropboxEntityList()
    {
        _selectEntity.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < _fieldEntitiesList.Count; i++)
        {
            options.Add(_fieldEntitiesList[i].name);
        }
        _selectEntity.AddOptions(options);

        _selectEntity.RefreshShownValue();
        _selectEntity.value = 0;
    }
    // 디버그용 드랍박스 변경 리스너
    private void OnDropboxChanged(string dropboxName, int index)
    {
        if (dropboxName == "Effect")
        {
            _selecedtEffectIdx = index;
        }
        else if (dropboxName == "Entity")
        {
            _selectedEntitytIdx = index;
        }
    }
    // 디버그용 버튼 리스너
    private void OnEffectApplyButton()
    {
        switch (_effectOptions[_selecedtEffectIdx])
        {
            case "데미지":
                int rand = UnityEngine.Random.Range(0, 10);

                if (rand < 6)
                {
                    _fieldEntitiesList[_selectedEntitytIdx].TakeDamage(5);
                    ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Damage, "5");
                }
                else
                {
                    _fieldEntitiesList[_selectedEntitytIdx].TakeDamage(10);
                    ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Damage_Crit, "10");
                    ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Damage_Crit_Msg, "치명타!");
                }
                _entityStateList[_fieldEntitiesList[_selectedEntitytIdx]].SetHealth();
                break;
            case "출혈":
                CEffect effect = new CEffect(_bleed);
                _fieldEntitiesList[_selectedEntitytIdx].ApplyStatusEffect(effect);
                ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Bleed, "출혈");
                _entityStateList[_fieldEntitiesList[_selectedEntitytIdx]].SetEffect(EEffectType.Bleed, true);
                break;
            case "중독":
                effect = new CEffect(_blight);
                _fieldEntitiesList[_selectedEntitytIdx].ApplyStatusEffect(effect);
                ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Blight, "중독");
                _entityStateList[_fieldEntitiesList[_selectedEntitytIdx]].SetEffect(EEffectType.Blight, true);
                break;
            case "화상":
                effect = new CEffect(_burn);
                _fieldEntitiesList[_selectedEntitytIdx].ApplyStatusEffect(effect);
                ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Burn, "화상");
                _entityStateList[_fieldEntitiesList[_selectedEntitytIdx]].SetEffect(EEffectType.Burn, true);
                break;
            case "현기증":
                effect = new CEffect(_daze);
                _fieldEntitiesList[_selectedEntitytIdx].ApplyStatusEffect(effect);
                ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Daze, "추가");
                _entityStateList[_fieldEntitiesList[_selectedEntitytIdx]].SetEffect(EEffectType.Daze, true);
                break;
            case "기절":
                effect = new CEffect(_stun);
                _fieldEntitiesList[_selectedEntitytIdx].ApplyStatusEffect(effect);
                ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.Stun, "추가");
                _entityStateList[_fieldEntitiesList[_selectedEntitytIdx]].SetEffect(EEffectType.Stun, true);
                break;
            case "추가행동":
                _fieldEntitiesList[_selectedEntitytIdx].HasExtraTurn = true;
                ShowUIMessage(_fieldEntitiesList[_selectedEntitytIdx].gameObject, CStateText.EMessageType.None, "추가 행동 부여");
                break;
        }

        Debug.Log("속성 부여 완료");
    }
    #endregion
    private void ChangeState()
    {
        if (_roundState == RoundState.None || _roundState == RoundState.Start)
        {
            if (_turnList.Count == 0)
            {
                _roundState = RoundState.Ready;
            }
        }

        if (_roundState == RoundState.Ready)
        {
            if (_turnList.Count != 0)
            {
                _roundState = RoundState.Start;
            }
        }
    }
    private void ActiveState()
    {
        switch (_roundState)
        {
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

        for (int i = 0; i < _fieldEntitiesList.Count; i++)
        {
            // 보스 개체는 행동 횟수를 여러번 가집니다.
            for (int actions = 0; actions < _fieldEntitiesList[i].Actions; actions++)
            {
                _turnList.Add((_fieldEntitiesList[i], _fieldEntitiesList[i].TempEntitySpeed + UnityEngine.Random.Range(0, 7)));
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


            // 4 3 2 1 초상화 배치 및 이동
            if (i < 4)
            {
                portraitImage.Initialize(4, _turnList[i].entity.Type, _turnList[i].entity.TurnImage);
                portraitImage.Move(i + 1);
            }
            else
            {
                portraitImage.Initialize(i + 1, _turnList[i].entity.Type, _turnList[i].entity.TurnImage);
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

        _myTurn = ShowUIIcon(_turnList[0].entity.gameObject, _targetPrefab);

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
        }
    }
    // 엔티티 상태 체크
    private EntityState CheckState()
    {
        if (_currentTurnState == TurnState.Normal)
        {
            // DOT 데미지 들어오는거 체크 후 죽음 상태로 변경
            (int bleed, int blight, int burn) total = _turnList[0].entity.ConsumeDOTEffect();

            int[] dotList = new int[] { total.bleed, total.blight, total.burn };

            CStateText.EMessageType[] dotType = new CStateText.EMessageType[] { CStateText.EMessageType.Bleed, CStateText.EMessageType.Blight, CStateText.EMessageType.Burn };

            for (int i = 0, delay = 1; i < dotList.Length; i++)
            {
                if (dotList[i] == 0)
                {
                    continue;
                }
                ShowUIMessage(_turnList[0].entity.gameObject, dotType[i], dotList[i].ToString(), default, delay * 0.3f);
                delay++;
            }


            _entityStateList[_turnList[0].entity].SetHealth();

            ContainDot(_turnList[0].entity);
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
            ContainToken(_turnList[0].entity, EEffectType.Stun);
            return EntityState.Stun;
        }

        bool isDazed = _turnList[0].entity.TokenManager.ConsumeToken(EEffectType.Daze);

        if (isDazed)
        {
            ContainToken(_turnList[0].entity, EEffectType.Daze);
            return EntityState.Daze;
        }

        return EntityState.Success;
    }

    // 턴이 왔는데 도트뎀으로 죽은 적들 처리
    private IEnumerator CoDeath()
    {
        yield return new WaitForSeconds(_actionDelay);
        for (int i = 0; i < _fieldEntitiesList.Count; i++)
        {
            if (_turnList[0].entity.Equals(_fieldEntitiesList[i]))
            {
                RemoveDeathEntity(i);
                break;
            }
        }
        // 디버그용 드롭박스
        SetDropboxEntityList();
        MoveTurnOrder(true);
        _coroutine = null;
    }
    // 엔티티끼리 상호작용하고 죽은 엔티티를 확인
    private void CheckEntitiesDeath()
    {
        int prev = _fieldEntitiesList.Count;

        for (int i = _fieldEntitiesList.Count - 1; i >= 0; i--)
        {
            if (_fieldEntitiesList[i].IsDead)
            {
                RemoveDeathEntity(i);
            }
        }
        if (_fieldEntitiesList.Count != prev)
        {
            // 디버그용 드롭박스
            SetDropboxEntityList();
            MoveTurnOrder(true);
        }
    }
    // 죽은 적 리스트에서 제거
    private void RemoveDeathEntity(int index)
    {
        CTempEntity entity = _fieldEntitiesList[index];

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
        ShowUIIcon(_fieldEntitiesList[index].gameObject, _deathStampPrefab);
        ShowUIMessage(_fieldEntitiesList[index].gameObject, CStateText.EMessageType.None, "죽음의 일격!");
        Destroy(_fieldEntitiesList[index].gameObject, _portraitRemoveDelay);
        _entityStateList.Remove(_fieldEntitiesList[index]);
        _fieldEntitiesList.RemoveAt(index);
    }

    // 기절 : 현재 라운드의 차례를 넘깁니다.
    private IEnumerator CoStun()
    {
        ShowUIMessage(_turnList[0].entity.gameObject, CStateText.EMessageType.Stun, "기절!");
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
        ShowUIMessage(_turnList[0].entity.gameObject, CStateText.EMessageType.Daze, "현기증!");

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
            EEntityType entityType = _turnList[0].entity.Type;

            // 히어로일 때
            // 1.적에게 마우스를 올렸을 때 정보가 나와야 합니다.
            // 2.현재 턴인 히어로의 정보가 나와야합니다.
            if (entityType == EEntityType.Hero)
            {
                // 플레이어 UI [ TODO : 추후 한번만 변경되도록 구현해야합니다. ]
                _heroInfo.SetActive(true);
                _skillListPanel.SetActive(true);
                string heroId = _turnList[0].entity.EntityData.Id;
                
                Sprite slotBG = _slotBGDict[heroId];
                Sprite portrait = _heroProfilePortraitDict[heroId];

                for(int i = 0; i < _skillSlot.Count; i++)
                {
                    _skillSlot[i].GetComponent<Image>().sprite = slotBG;
                    _skillImage[i].sprite = _turnList[0].entity.SkillList[i].SkillIcon;
                }

                CHeroDataSO heroSO = (CHeroDataSO) _turnList[0].entity.EntityData;
                _heroName.text = heroSO.DefaultName;
                _heroPortrait.GetComponent<Image>().sprite = portrait;                

                // 적 UI
                mousePos = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(mousePos);

                Physics.Raycast(ray, out RaycastHit hitInfo);

                if (hitInfo.collider != null)
                {
                    // 임시 : 현재 TempEntity라서 추후에 뜯어고쳐야합니다.
                    bool isEntity = hitInfo.collider.gameObject.TryGetComponent<CTempEntity>(out CTempEntity hitEntity);

                    if (isEntity)
                    {
                        if (hitEntity.Type == EEntityType.Enemy)
                        {
                            // 임시 적용 추후 DataSO를 활용해야합니다.
                            _enemyName.text = hitEntity.EntityData.Name;
                            _enemyPortrait.GetComponent<Image>().sprite = _enemyProfilePortraitDict[hitEntity.EntityData.Id];
                            _enemyMaxHealth.text = hitEntity.EntityData.MaxHealth.ToString();
                            _enemyCurHealth.text = hitEntity.CurrentHealth.ToString();
                            _enemyHpBarFill.fillAmount = Mathf.Clamp01((float)hitEntity.CurrentHealth / (float)hitEntity.EntityData.MaxHealth);

                            _enemySpeed.text = hitEntity.EntityData.Speed.ToString();
                            _enemyBleedResist.text = hitEntity.EntityData.Resistance.Bleed.ToString();
                            _enemyBlightResist.text = hitEntity.EntityData.Resistance.Blight.ToString();
                            _enemyBurnResist.text = hitEntity.EntityData.Resistance.Burn.ToString();
                            _enemyStunResist.text = hitEntity.EntityData.Resistance.Stun.ToString();
                            _enemyMoveResist.text = hitEntity.EntityData.Resistance.Move.ToString();
                            _enemyWeakResist.text = hitEntity.EntityData.Resistance.Debuff.ToString();

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
                else
                {
                    _enemyInfo.SetActive(false);
                    _enemyStatus.SetActive(false);
                }
            }
            // 적인경우 
            else
            {
                _heroInfo.SetActive(false);
                _skillListPanel.SetActive(false);

                
                CTempEntity entity = _turnList[0].entity;

                // 이름 반영
                _enemyName.text = entity.EntityData.Name;
                // 초상화 반영
                _enemyPortrait.GetComponent<Image>().sprite = _enemyProfilePortraitDict[entity.EntityData.Id];

                // 체력 반영
                _enemyMaxHealth.text = entity.EntityData.MaxHealth.ToString();
                _enemyCurHealth.text = entity.CurrentHealth.ToString();
                _enemyHpBarFill.fillAmount = Mathf.Clamp01((float)entity.CurrentHealth / (float)entity.EntityData.MaxHealth);
                
                
                _enemyInfo.SetActive(true);
            }

            doAction = _turnList[0].entity.Action();
            yield return null;
        }

        ShowUIMessage(_turnList[0].entity.gameObject, CStateText.EMessageType.None, !_pass ? "행동" : "턴넘김");
        _pass = false;

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
            ShowUIMessage(_turnList[0].entity.gameObject, CStateText.EMessageType.None, "추가 행동");
        }
    }
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
            if (_turnList[0].entity.HasExtraTurn)
            {
                _turnList[0].entity.HasExtraTurn = false;
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
    #region UI Message Print
    // 상태 메세지 출력
    private void ShowUIMessage(GameObject target, CStateText.EMessageType messageType, string text, Vector3 offset = default, float delay = 0f)
    {
        GameObject stateTextObj = Instantiate(_stateTextPrefab, _stateTextRoot.transform);
        CStateText stateText = stateTextObj.GetComponent<CStateText>();

        if (delay == 0f)
        {
            stateText.SetText(_canvas, target, messageType, text, offset);
        }
        else
        {
            StartCoroutine(CoDelayShow(() => stateText.SetText(_canvas, target, messageType, text, offset), delay));
        }
    }
    // 현재는 죽음의 일격정도만 있습니다.
    // TODO : 스트레스, 스트레스 회복
    private GameObject ShowUIIcon(GameObject target, GameObject prefab, float delay = 0f)
    {
        GameObject iconPrefab = Instantiate(prefab, _iconPrefabRoot.transform);
        CIcon icon = iconPrefab.GetComponent<CIcon>();


        if (delay == 0f)
        {
            icon.SetIcon(_canvas, target);
        }
        else
        {
            StartCoroutine(CoDelayShow(() => icon.SetIcon(_canvas, target), delay));
        }

        return iconPrefab;
    }
    // 메세지를 일정시간뒤에 출력하는 코루틴
    private IEnumerator CoDelayShow(Action action, float delay)
    {

        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
    #endregion
    private void NextRound()
    {
        _round++;
        _roundText.text = _round.ToString();
    }
    // 임시 도트 체크
    private void ContainDot(CTempEntity entity)
    {
        bool hasBleed = false;
        bool hasBlight = false;
        bool hasBurn = false;

        for (int i = 0; i < entity.DotEffectList.Count; i++)
        {
            if (entity.DotEffectList[i].EffectType == EEffectType.Bleed)
            {
                hasBleed = true;
            }
            else if (entity.DotEffectList[i].EffectType == EEffectType.Blight)
            {
                hasBlight = true;
            }
            else if (entity.DotEffectList[i].EffectType == EEffectType.Burn)
            {
                hasBurn = true;
            }
        }
        if (!hasBleed)
        {
            _entityStateList[entity].SetEffect(EEffectType.Bleed, false);
        }
        if (!hasBlight)
        {
            _entityStateList[entity].SetEffect(EEffectType.Blight, false);
        }
        if (!hasBurn)
        {
            _entityStateList[entity].SetEffect(EEffectType.Burn, false);
        }
    }
    // 토큰 체크
    private void ContainToken(CTempEntity entity, EEffectType effectType)
    {
        if (!entity.TokenManager.HasToken(effectType))
        {
            _entityStateList[entity].SetEffect(effectType, false);
        }
    }

}
