using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class CCombatUI : MonoBehaviour
{
    #region Inspector
    [Header("Draw Canvas")]
    [SerializeField] private Canvas _canvas;

    [Header("CombatMaanger")]
    [SerializeField] private CCombatManager _combatManager;

    [Header("Combat Panel")]
    [SerializeField] private GameObject _combatFrontPanel;
    [SerializeField] private GameObject _meltdownPanel;
    [SerializeField] private GameObject _meltDownFront;
    [SerializeField] private GameObject _resolutePanel;
    [SerializeField] private GameObject _resoluteFront;
    [SerializeField] private GameObject _combatBorder;
    [SerializeField] private GameObject _combatTriangle;
    [SerializeField] private GameObject _leftHit;
    [SerializeField] private GameObject _rightHit;
    [SerializeField] private TextMeshProUGUI _heroTemptation;
    [SerializeField] private TextMeshProUGUI _heroTemptationResult;
    // 현재 동작중인 스킬 이름
    [SerializeField] private TextMeshProUGUI _currentSkillName;

    [Header("Hero Infomation")]
    // 영웅 정보 패널
    [SerializeField] private GameObject _heroInfo;
    [SerializeField] private GameObject _infoTriangle;
    [SerializeField] private GameObject _heroPortrait;
    [SerializeField] private TextMeshProUGUI _heroName;

    [SerializeField] private GameObject _heroSkillPanel;
    [SerializeField] private TextMeshProUGUI _heroSkillMainDesc;
    [SerializeField] private TextMeshProUGUI _heroSkillSubDesc;

    [Header("Skill Panel")]
    // 영웅 스킬 항목
    [SerializeField] private GameObject _skillListPanel;
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
    // 자리 변경과 한턴쉬기 그리고 선택 UI
    [SerializeField] private GameObject _rankChangeBtn;
    [SerializeField] private GameObject _passBtn;
    [SerializeField] private GameObject _cursorDiamond;

    [Header("Enemy Infomation")]
    [SerializeField] private GameObject _enemyInfo;
    [SerializeField] private GameObject _enemyPortrait;
    [SerializeField] private TextMeshProUGUI _enemyName;
    // 적 상세항목 패널
    [SerializeField] private GameObject _enemyResistStatus;
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

    [Header("Round Text")]
    [SerializeField] private TextMeshProUGUI _roundText;

    [Header("Entity State Prefab")]
    [SerializeField] private GameObject _entitiesStateRoot;
    [SerializeField] private GameObject _entitiesStatePrefab;

    [Header("Entity Indicator")]
    [SerializeField] private GameObject _entitiesIndicatorRoot;
    [SerializeField] private GameObject _indicatorPrefab;

    // 턴 초상화 이미지 프리팹과 보관 위치
    [Header("Turnorder Prefab")]
    [SerializeField] private GameObject _turnOrderRoot;
    [SerializeField] private GameObject _turnOrderPrefab;

    [Header("Icon Prefab Root")]
    [SerializeField] private GameObject _iconRoot;

    [Header("TurnGuide Root")]
    [SerializeField] private GameObject _turnGuideRoot;

    [Header("Text Prefab Root")]
    [SerializeField] private GameObject _textRoot;
    #endregion



    #region Field
    private int _round = 0;
    private bool _pass = false;
    private bool _rankChange = false;
    private bool _cursorRank = false;
    private Coroutine _indicatorCoroutine = null;
    private List<Object> _checkList = new List<Object>();
    #endregion

    #region Property
    public CEntity TurnEntity => _combatManager.MyTurn;

    public bool Pass
    {
        get { return _pass; }
        set { _pass = value; }
    }
    public bool Change
    {
        get { return _rankChange; }
        set { _rankChange = value; }
    }
    public RectTransform HeroTemptationTextTr => _heroTemptation.gameObject.GetComponent<RectTransform>();
    #endregion

    private void Awake()
    {
        #region Null Check
        _checkList.Add(_canvas);
        _checkList.Add(_combatManager);

        _checkList.Add(_combatFrontPanel);
        _checkList.Add(_resolutePanel);
        _checkList.Add(_meltdownPanel);
        _checkList.Add(_combatTriangle);
        _checkList.Add(_meltDownFront);
        _checkList.Add(_resoluteFront);
        _checkList.Add(_leftHit);
        _checkList.Add(_rightHit);
        _checkList.Add(_heroTemptation);
        _checkList.Add(_heroTemptationResult);

        _checkList.Add(_heroInfo);
        _checkList.Add(_heroPortrait);
        _checkList.Add(_heroName);

        _checkList.Add(_heroSkillPanel);
        _checkList.Add(_heroSkillMainDesc);
        _checkList.Add(_heroSkillSubDesc);

        _checkList.Add(_skillListPanel);

        _checkList.Add(_currentSkillName);

        _checkList.Add(_cursorSkillInfo);
        _checkList.Add(_cursorSkill);
        _checkList.Add(_cursorSkillName);

        _checkList.Add(_AlliesRankPanel);
        for (int i = 0; i < _AlliesRank.Count; i++)
        {
            _checkList.Add(_AlliesRank[i]);
        }
        _checkList.Add(_targetRankPanel);
        for (int i = 0; i < _targetRank.Count; i++)
        {
            _checkList.Add(_targetRank[i]);
        }

        for (int i = 0; i < _skillSlot.Count; i++)
        {
            _checkList.Add(_skillSlot[i]);
        }
        for (int i = 0; i < _skillImage.Count; i++)
        {
            _checkList.Add(_skillImage[i]);
        }

        _checkList.Add(_combatItemSlot);
        _checkList.Add(_combatItemImage);

        _checkList.Add(_rankChangeBtn);
        _checkList.Add(_passBtn);
        _checkList.Add(_cursorDiamond);

        _checkList.Add(_enemyInfo);
        _checkList.Add(_enemyPortrait);
        _checkList.Add(_enemyName);

        _checkList.Add(_enemyResistStatus);
        _checkList.Add(_enemyType);

        _checkList.Add(_enemyMaxHealth);
        _checkList.Add(_enemyCurHealth);
        _checkList.Add(_enemyHpBarFill);

        _checkList.Add(_enemySpeed);
        _checkList.Add(_enemyBleedResist);
        _checkList.Add(_enemyBlightResist);
        _checkList.Add(_enemyBurnResist);
        _checkList.Add(_enemyStunResist);
        _checkList.Add(_enemyMoveResist);
        _checkList.Add(_enemyWeakResist);

        _checkList.Add(_roundText);

        _checkList.Add(_entitiesStateRoot);
        _checkList.Add(_entitiesStatePrefab);

        _checkList.Add(_entitiesIndicatorRoot);
        _checkList.Add(_indicatorPrefab);

        _checkList.Add(_turnOrderRoot);
        _checkList.Add(_turnOrderPrefab);

        _checkList.Add(_iconRoot);
        _checkList.Add(_turnGuideRoot);
        _checkList.Add(_textRoot);

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
        {
            enabled = false;
            return;
        }
        #endregion

        for (int i = 0; i < _skillSlot.Count; i++)
        {
            int idx = i;
            _skillSlot[i].GetComponent<Button>().onClick.AddListener(() => OnSelectSkill(idx));
        }

        if (CInputDispatcher.Instance != null)
        {
            // 스킬 사용 키 등록
            CInputDispatcher.Instance.OnSkill += OnSelectSkill;
            CPrint.Log("디스패쳐와 연결 완료");
        }
    }

    // 엔티티 현재 상태를 확인할 수 있는 UI 추가
    public CEntityState CreateStateUI(CEntity entity)
    {
        // 엔티티 상태창 추가
        CEntityState state = Instantiate(_entitiesStatePrefab, _entitiesStateRoot.transform).GetComponent<CEntityState>();
        state.Initialize(_canvas, entity.gameObject);

        return state;
    }
    // 현재 턴 및 어디를 가리키고 있는지 확인할 수 있는 지시등 UI 추가
    public CIndicator CreateIndicatorUI(CEntity entity)
    {
        CIndicator indicator = Instantiate(_indicatorPrefab, _entitiesIndicatorRoot.transform).GetComponent<CIndicator>();
        indicator.Initialize(_canvas, entity.gameObject);

        return indicator;
    }
    // 턴순서 초상화 리스트 생성
    public List<GameObject> CreateTurnOrderPortraitList(List<(int id, CEntity entity, int speed)> turnList)
    {
        List<GameObject> turnPortraitList = new List<GameObject>();

        // 턴 이미지 세팅 및 생성
        for (int i = 0; i < turnList.Count; i++)
        {
            GameObject portraitPrefab = Instantiate(_turnOrderPrefab, _turnOrderRoot.transform);

            CPortraitImage portraitImage = portraitPrefab.GetComponent<CPortraitImage>();

            Sprite portrait = turnList[i].entity.EntityData.PortraitTurnOrder;
            // 초상화 마커 그리기
            bool isHero = turnList[i].entity is CPlayer;
            EEntityType entityType = isHero ? EEntityType.Hero : EEntityType.Enemy;

            // 4 3 2 1 초상화 배치 및 이동
            if (i < 4)
            {
                portraitImage.Initialize(_combatManager, turnList[i].id, 4, entityType, portrait);
                portraitImage.Move(i + 1);
            }
            else
            {
                portraitImage.Initialize(_combatManager, turnList[i].id, i + 1, entityType, portrait);
            }

            turnPortraitList.Add(portraitPrefab);
        }

        return turnPortraitList;
    }
    // 현기증으로 인한 턴 밀림 시 초상화 추가 생성
    public GameObject CopyTurnOrderPortrait(GameObject portrait)
    {
        GameObject copyObj = Instantiate(portrait, _turnOrderRoot.transform);
        return copyObj;
    }
    // 초상화 이동
    public void MoveTurnOrder(List<GameObject> turnPortraitList, bool isDirty = false)
    {
        // 죽는 상태에선 단순 하나의 요소만 변경되는 것이 아니기 때문에 리스트와 싱크를 맞춰야한다.
        if (isDirty)
        {
            for (int i = 0; i < turnPortraitList.Count; i++)
            {
                if (turnPortraitList.Count - 1 < i)
                {
                    break;
                }

                CPortraitImage portraitImage = turnPortraitList[i].GetComponent<CPortraitImage>();

                portraitImage.Move(i + 1);
            }
            return;
        }


        for (int i = 0; i < turnPortraitList.Count; i++)
        {
            if (turnPortraitList.Count - 1 < i)
            {
                break;
            }

            CPortraitImage portraitImage = turnPortraitList[i].GetComponent<CPortraitImage>();

            portraitImage.Move(i);
        }
        CPortraitImage.IsDirty = false;
    }
    // 다음 라운드로 변경
    public void NextRound(int turnListCount)
    {
        if (turnListCount == 0)
        {
            return;
        }

        _round++;
        _roundText.text = _round.ToString();
    }
    // 버튼을 누른채로 몇초 지나면 패스 적용
    public void OnPressActionPass()
    {
        if (TurnEntity == null)
        {
            return;
        }

        if (TurnEntity is not CPlayer)
        {
            return;
        }

        _combatManager.MyTurn.CancelAction();
        _combatManager.MyTurn.TryBeginAction();

        Dictionary<CEntity, CIndicator> indicators = _combatManager.EntitiesIndicator;

        // 지시등 초기화
        for (int i = 0; i < _combatManager.EntityList.Count; i++)
        {
            CEntity entity = _combatManager.EntityList[i].entity;

            if (indicators.TryGetValue(entity, out CIndicator indicator))
            {
                indicator.ToggleMouseEvent(false);

                // 본인이면 제외
                if (entity.Equals(_combatManager.MyTurn))
                {
                    indicator.ViewMyturnIndicator();
                    continue;
                }

                indicator.HideIndicator();
            }
        }


        // 커서 이동
        RectTransform rectTr = _passBtn.GetComponent<RectTransform>();
        RectTransform cursorTr = _cursorDiamond.GetComponent<RectTransform>();
        cursorTr.anchoredPosition = rectTr.anchoredPosition;

        _cursorRank = false;
        _cursorDiamond.SetActive(true);
        _cursorSkill.SetActive(false);
    }
    // 턴넘김
    public void ActionPass()
    {
        _pass = true;
        CloseHeroInfomation();
    }

    // 자리 변경 UI
    public void OnPressRankChange()
    {
        if (TurnEntity == null)
        {
            return;
        }

        if (TurnEntity is not CPlayer)
        {
            return;
        }

        _combatManager.MyTurn.CancelAction();
        _combatManager.MyTurn.TryBeginAction();

        Dictionary<CEntity, CIndicator> indicators = _combatManager.EntitiesIndicator;
        CPlayer myturnHero = _combatManager.MyTurn as CPlayer;

        // 지시등 초기화
        for (int i = 0; i < _combatManager.EntityList.Count; i++)
        {
            CEntity entity = _combatManager.EntityList[i].entity;

            if (indicators.TryGetValue(entity, out CIndicator indicator))
            {
                indicator.ToggleMouseEvent(false);

                // 본인이면 제외
                if (entity.Equals(myturnHero))
                {
                    indicator.ViewMyturnIndicator();
                    continue;
                }

                indicator.HideIndicator();
            }
        }

        // 현재 위치에서 이동할 수 있는 지시등만 켜기
        int rankIdx = (int)System.MathF.Log((float)myturnHero.CurrentRank, 2);
        int forward = myturnHero.HeroData.ForwardMovement;
        int backward = myturnHero.HeroData.BackwardMovement;

        int start = Mathf.Max(rankIdx - forward, 0);
        int end = Mathf.Min(rankIdx + backward, 3);

        for (int i = start; i <= end; i++)
        {
            IReadOnlyDictionary<ERank, CPlayer> heroRank = _combatManager.HeroRank;

            if (heroRank.TryGetValue((ERank)(1 << i), out CPlayer hero))
            {
                if (indicators.TryGetValue(hero, out CIndicator indicator))
                {
                    
                    // 본인이면 제외
                    if (hero.Equals(myturnHero))
                    {
                        indicator.ViewMyturnIndicator();
                        continue;
                    }
                    
                    indicator.ToggleMouseEvent(true);
                }
            }
        }

        // 커서 이동
        RectTransform rectTr = _rankChangeBtn.GetComponent<RectTransform>();
        RectTransform cursorTr = _cursorDiamond.GetComponent<RectTransform>();
        cursorTr.anchoredPosition = rectTr.anchoredPosition;

        _cursorRank = true;
        _cursorDiamond.SetActive(true);
        _cursorSkill.SetActive(false);
    }

    // 스킬 선택
    private void OnSelectSkill(int idx)
    {
        if (TurnEntity == null)
        {
            return;
        }

        if (!_skillListPanel.activeSelf)
        {
            return;
        }

        if (TurnEntity is not CPlayer)
        {
            return;
        }

        RectTransform SkillSlot = _skillSlot[idx].GetComponent<RectTransform>();
        RectTransform cursorRectTr = _cursorSkill.GetComponent<RectTransform>();
        cursorRectTr.anchoredPosition = SkillSlot.anchoredPosition;

        CPlayer hero = TurnEntity as CPlayer;

        CSkillDataSO skillSO = hero.EquippedSkills[idx];
        ERank usableRank = skillSO.UsableRank;
        ERank targetRank = skillSO.TargetableRank;

        _cursorSkillName.text = skillSO.SkillName;

        CHeroSkillSO heroSkillSO = skillSO as CHeroSkillSO;

        // 현재 스킬의 랭크 표시
        bool aliyRank1 = ((usableRank & ERank.Rank1) != 0);
        bool aliyRank2 = ((usableRank & ERank.Rank2) != 0);
        bool aliyRank3 = ((usableRank & ERank.Rank3) != 0);
        bool aliyRank4 = ((usableRank & ERank.Rank4) != 0);

        bool targetRank1 = ((targetRank & ERank.Rank1) != 0);
        bool targetRank2 = ((targetRank & ERank.Rank2) != 0);
        bool targetRank3 = ((targetRank & ERank.Rank3) != 0);
        bool targetRank4 = ((targetRank & ERank.Rank4) != 0);

        List<bool> alliesRankList = new List<bool> { aliyRank1, aliyRank2, aliyRank3, aliyRank4 };
        List<bool> targetRankList = new List<bool> { targetRank1, targetRank2, targetRank3, targetRank4 };

        if (heroSkillSO.Target != EHeroSkillTarget.Self)
        {
            for (int i = 0; i < alliesRankList.Count; i++)
            {
                _AlliesRank[i].enabled = alliesRankList[i];
                _targetRank[i].enabled = targetRankList[i];
            }

            _targetRankPanel.SetActive((targetRank & (ERank.Rank1 | ERank.Rank2 | ERank.Rank3 | ERank.Rank4)) == 0 ? false : true);
        }
        else
        {
            for (int i = 0; i < alliesRankList.Count; i++)
            {
                _AlliesRank[i].enabled = targetRankList[i];
            }

            _targetRankPanel.SetActive(false);
        }

        _cursorSkillInfo.SetActive(true);
        _cursorSkill.SetActive(true);
        _cursorDiamond.SetActive(false);
        _cursorRank = false;

        // 영웅의 스킬 사전준비 모션
        hero.SelectSkill(idx);

        if (_indicatorCoroutine == null)
        {
            _indicatorCoroutine = StartCoroutine(CoViewTarget(heroSkillSO));
        }
        else
        {
            StopCoroutine(_indicatorCoroutine);
            _indicatorCoroutine = null;
            _indicatorCoroutine = StartCoroutine(CoViewTarget(heroSkillSO));
        }
    }

    private IEnumerator CoViewTarget(CHeroSkillSO heroSkillSO)
    {
        Dictionary<CEntity, CIndicator> indicators = _combatManager.EntitiesIndicator;

        // 지시등 초기화
        for (int i = 0; i < _combatManager.EntityList.Count; i++)
        {
            CEntity entity = _combatManager.EntityList[i].entity;

            if (indicators.TryGetValue(entity, out CIndicator indicator))
            {
                CHeroTargetView view = entity.gameObject.GetComponent<CHeroTargetView>();

                yield return null;

                indicator.SetView(view);
                indicator.ToggleMouseEvent(false);

                // 본인이면 제외
                if (entity.Equals(_combatManager.MyTurn))
                {
                    continue;
                }

                indicator.HideIndicator();
            }
        }

        // 자신의 스킬이 어떤 종류인지에 따라 지시등이 변합니다.
        if (
            heroSkillSO.Target == EHeroSkillTarget.Self ||
            heroSkillSO.Target == EHeroSkillTarget.Ally ||
            heroSkillSO.Target == EHeroSkillTarget.AllyOrSelf ||
            heroSkillSO.Target == EHeroSkillTarget.AllAllies
          )
        {
            _combatManager.MyTurnIndicator.ViewBuffIndicator();
        }
        else
        {
            _combatManager.MyTurnIndicator.ViewMyturnIndicator();
        }

        for (int i = 0; i < _combatManager.EntityList.Count; i++)
        {
            if (indicators.TryGetValue(_combatManager.EntityList[i].entity, out CIndicator indicator))
            {
                if (_combatManager.EntityList[i].entity == _combatManager.MyTurn)
                {
                    continue;
                }

                if (indicator.isHighlight)
                {
                    indicator.ViewTargetIndicator();
                }
            }
        }
    }

    public void HideAllIndicator()
    {
        Dictionary<CEntity, CIndicator> indicators = _combatManager.EntitiesIndicator;

        for (int i = 0; i < _combatManager.EntityList.Count; i++)
        {
            if (indicators.TryGetValue(_combatManager.EntityList[i].entity, out CIndicator indicator))
            {
                indicator.HideIndicator();
                indicator.ToggleMouseEvent(false);
            }
        }
    }

    /// <summary>
    /// 영웅 정보 및 스킬 인터페이스 활성화
    /// </summary>
    /// <param name="entity">엔티티 대상</param>
    public void OpenHeroInfomation(CPlayer hero, CHeroBattleInput battleInput)
    {
        if (hero == null)
        {
            return;
        }

        CHeroDataSO heroSO = hero.HeroData;

        for (int i = 0; i < _skillSlot.Count; i++)
        {
            _skillSlot[i].GetComponent<Image>().sprite = heroSO.SkillSlotBackground;
            _skillImage[i].sprite = hero.EquippedSkills[i].SkillIcon;
            battleInput.SelectSkill(i);
        }

        _heroName.text = heroSO.DefaultName;
        _heroPortrait.GetComponent<Image>().sprite = heroSO.PortraitInfo;

        _heroInfo.SetActive(true);
        _skillListPanel.SetActive(true);
        _infoTriangle.SetActive(true);
    }
    public void CloseHeroInfomation()
    {
        _heroInfo.SetActive(false);
        _skillListPanel.SetActive(false);
        _infoTriangle.SetActive(false);
        _cursorSkillInfo.SetActive(false);
        _cursorSkill.SetActive(false);
        _cursorDiamond.SetActive(false);
        _currentSkillName.gameObject.SetActive(false);
    }
    /// <summary>
    /// 적 정보 탭 활성화
    /// </summary>
    /// <param name="enemy">마우스로 바라보는 대상</param>
    /// <param name="isFullInfomation">플레이어 차례 : 모든정보 / 적 차례 : 저항정보 제외</param>
    public void OpenEnemyInfomation(CEnemy enemy, bool isFullInfomation)
    {
        if (enemy == null)
        {
            return;
        }

        CEnemyDataSO enemyDataSO = enemy.EnemyData;

        _enemyName.text = enemyDataSO.Name;
        _enemyPortrait.GetComponent<Image>().sprite = enemyDataSO.PortraitInfo;
        _enemyMaxHealth.text = enemyDataSO.MaxHealth.ToString();
        _enemyCurHealth.text = enemy.CurrentHp.ToString();
        _enemyHpBarFill.fillAmount = Mathf.Clamp01((float)enemy.CurrentHp / (float)enemyDataSO.MaxHealth);

        _enemySpeed.text = enemyDataSO.Speed.ToString();
        _enemyInfo.SetActive(true);

        if (isFullInfomation)
        {
            _enemyBleedResist.text = enemyDataSO.Resistance.Bleed.ToString();
            _enemyBlightResist.text = enemyDataSO.Resistance.Blight.ToString();
            _enemyBurnResist.text = enemyDataSO.Resistance.Burn.ToString();
            _enemyStunResist.text = enemyDataSO.Resistance.Stun.ToString();
            _enemyMoveResist.text = enemyDataSO.Resistance.Move.ToString();
            _enemyWeakResist.text = enemyDataSO.Resistance.Debuff.ToString();
            _enemyResistStatus.SetActive(true);
        }
    }
    public void CloseEnemyInfomation()
    {
        _enemyInfo.SetActive(false);
        _enemyResistStatus.SetActive(false);
    }
    // 스킬 사용 시 처리
    public void UseSkill(CSkillDataSO skillDataSO, bool onlyUseSkillName = false)
    {
        CloseHeroInfomation();

        _currentSkillName.text = skillDataSO.SkillName;
        OpenCombatPanel();
        _currentSkillName.gameObject.SetActive(true);

        if (!onlyUseSkillName)
        {
            _combatBorder.SetActive(true);
            _combatTriangle.SetActive(true);
        }
    }
    // 전투 패널 활성화
    public void OpenCombatPanel()
    {
        ResetCombatPanel();
        _combatFrontPanel.SetActive(true);
    }
    public void CloseCombatPanel()
    {
        _combatFrontPanel.SetActive(false);
    }
    public void ResetCombatPanel()
    {
        _combatFrontPanel.SetActive(false);
        _meltdownPanel.SetActive(false);
        _resolutePanel.SetActive(false);
        _combatBorder.SetActive(false);
        _combatTriangle.SetActive(false);
        _leftHit.SetActive(false);
        _rightHit.SetActive(false);
        _heroTemptation.gameObject.SetActive(false);
        _heroTemptationResult.gameObject.SetActive(false);
        _currentSkillName.gameObject.SetActive(false);
        _meltDownFront.SetActive(false);
        _resoluteFront.SetActive(false);
    }
    public void AttackLeft()
    {
        _rightHit.SetActive(false);
        _leftHit.SetActive(true);
    }
    public void AttackRight()
    {
        _rightHit.SetActive(true);
        _leftHit.SetActive(false);
    }
    // 누군가의 의지가 시험받고 있습니다.
    public void OpenTemptation(CPlayer hero)
    {
        ResetCombatPanel();

        _combatFrontPanel.SetActive(true);
        _heroTemptation.gameObject.SetActive(true);

        _heroTemptation.text = hero.HeroData.DefaultName + "의 의지가 시험받고 있습니다...";
    }
    public void OpenMeltDown()
    {
        ResetCombatPanel();

        _heroTemptationResult.gameObject.SetActive(true);
        _combatFrontPanel.SetActive(true);
        _meltdownPanel.SetActive(true);
        _combatBorder.SetActive(true);
        _combatTriangle.SetActive(true);
        _meltDownFront.SetActive(true);
        _heroTemptationResult.text = "붕괴";
        _heroTemptationResult.color = new Color32(13, 179, 218, 255);
    }
    // 영웅의 기상 연출
    public void OpenResolute()
    {
        ResetCombatPanel();

        _heroTemptationResult.gameObject.SetActive(true);
        _combatFrontPanel.SetActive(true);
        _resolutePanel.SetActive(true);
        _combatBorder.SetActive(true);
        _combatTriangle.SetActive(true);
        _resoluteFront.SetActive(true);
        _heroTemptationResult.text = "결의";
        _heroTemptationResult.color = new Color32(252, 219, 248, 255);
    }

    public void TogglePrefabRoot(bool toggle)
    {
        _turnOrderRoot.SetActive(toggle);
        _entitiesStateRoot.SetActive(toggle);
        _iconRoot.SetActive(toggle);
        _textRoot.SetActive(toggle);
        _turnGuideRoot.SetActive(toggle);
        _entitiesIndicatorRoot.SetActive(toggle);
    }
    public void ToggleTurnGuideRoot(bool toggle)
    {
        _turnGuideRoot.SetActive(toggle);
    }
    public void ToggleEntityStateRoot(bool toggle)
    {
        _entitiesStateRoot.SetActive(toggle);
    }

    public void ToggleIndicatorRoot(bool toggle)
    {
        _entitiesIndicatorRoot.SetActive(toggle);
    }
}
