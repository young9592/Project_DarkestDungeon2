using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CCombatUI : MonoBehaviour
{
    [Header("Draw Canvas")]
    [SerializeField] private Canvas _canvas;

    [Header("CombatMaanger")]
    [SerializeField] private CBasicCombatManager _combatManager;

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
    [SerializeField] private GameObject _entityStateRoot;
    [SerializeField] private GameObject _entityStatePrefab;

    // 턴 초상화 이미지 프리팹과 보관 위치
    [Header("Turnorder Prefab")]
    [SerializeField] private GameObject _turnOrderRoot;
    [SerializeField] private GameObject _turnOrderPrefab;



    #region Field
    private int _round = 0;
    private bool _pass = false;
    private List<Object> _checkList = new List<Object>();
    #endregion

    #region Property
    public CEntity TurnEntity => _combatManager.MyTurn;
    public bool Pass { get; set; }
    #endregion

    private void Awake()
    {
        #region Null Check
        _checkList.Add(_canvas);
        _checkList.Add(_combatManager);

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

        _checkList.Add(_entityStateRoot);
        _checkList.Add(_entityStatePrefab);

        _checkList.Add(_turnOrderRoot);
        _checkList.Add(_turnOrderPrefab);

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

        _passBtn.GetComponent<Button>().onClick.AddListener(OnPressActionPass);


        if (CInputDispatcher.Instance != null)
        {
            // 스킬 사용 키 등록
            CInputDispatcher.Instance.OnSkill += OnSelectSkill;
            CPrint.Log("디스패쳐와 연결 완료");
        }
    }

    /// <summary>
    /// 엔티티 현재 상태를 확인할 수 있는 UI 추가
    /// </summary>
    /// <param name="entity">적용할 대상</param>
    /// <returns></returns>
    public CEntityState CreateStateUI(CEntity entity)
    {
        // 엔티티 상태창 추가
        CEntityState entityState = Instantiate(_entityStatePrefab, _entityStateRoot.transform).GetComponent<CEntityState>();
        entityState.Initialize(_canvas, entity.gameObject);

        return entityState;
    }
    /// <summary>
    /// 턴순서 초상화 리스트 생성
    /// </summary>
    /// <param name="turnList">현재 턴 순서 목록</param>
    /// <returns></returns>
    public List<GameObject> CreateTurnOrderPortraitList(List<(CEntity entity, int speed)> turnList)
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
                portraitImage.Initialize(4, entityType, portrait);
                portraitImage.Move(i + 1);
            }
            else
            {
                portraitImage.Initialize(i + 1, entityType, portrait);
            }

            turnPortraitList.Add(portraitPrefab);
        }

        return turnPortraitList;
    }
    /// <summary>
    /// 현기증으로 인한 턴 밀림 시 초상화 추가 생성
    /// </summary>
    /// <param name="portrait">복사할 대상</param>
    /// <returns></returns>
    public GameObject CopyTurnOrderPortrait(GameObject portrait)
    {
        GameObject copyObj = Instantiate(portrait, _turnOrderRoot.transform);
        return copyObj;
    }
    /// <summary>
    /// 초상화 이동
    /// </summary>
    /// <param name="isDirty">죽은 엔티티가 있는지 체크</param>
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

    /// <summary>
    /// 다음 라운드로 변경
    /// </summary>
    public void NextRound(int turnListCount)
    {
        if (turnListCount == 0)
        {
            return;
        }

        _round++;
        _roundText.text = _round.ToString();
    }


    /// <summary>
    /// 턴넘김 선택 UI
    /// </summary>
    public void OnSelectActionPass()
    {

    }

    // 턴넘김
    public void OnPressActionPass()
    {
        if (TurnEntity == null)
        {
            _pass = false;
            return;
        }

        _pass = true;
        TurnEntity.TryBeginAction();
    }

    /// <summary>
    /// 자리변경 선택 UI
    /// </summary>
    public void OnSelectRankChange()
    {

    }

    public void OnPressRankChange()
    {

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

        _AlliesRank[0].enabled = ((usableRank & ERank.Rank1) != 0 ? true : false);
        _AlliesRank[1].enabled = ((usableRank & ERank.Rank2) != 0 ? true : false);
        _AlliesRank[2].enabled = ((usableRank & ERank.Rank3) != 0 ? true : false);
        _AlliesRank[3].enabled = ((usableRank & ERank.Rank4) != 0 ? true : false);

        _targetRank[0].enabled = ((targetRank & ERank.Rank1) != 0 ? true : false);
        _targetRank[1].enabled = ((targetRank & ERank.Rank2) != 0 ? true : false);
        _targetRank[2].enabled = ((targetRank & ERank.Rank3) != 0 ? true : false);
        _targetRank[3].enabled = ((targetRank & ERank.Rank4) != 0 ? true : false);

        _targetRankPanel.SetActive((targetRank & (ERank.Rank1 | ERank.Rank2 | ERank.Rank3 | ERank.Rank4)) == 0 ? false : true);

        _cursorSkillInfo.SetActive(true);
        _cursorSkill.SetActive(true);
        _cursorDiamond.SetActive(false);

        // 영웅의 스킬 사전준비 모션
        hero.SelectSkill(idx);
    }
    /// <summary>
    /// 영웅 정보 및 스킬 인터페이스 활성화
    /// </summary>
    /// <param name="entity">엔티티 대상</param>
    public void OpenHeroInfomation(CPlayer hero)
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
        }

        _heroName.text = heroSO.DefaultName;
        _heroPortrait.GetComponent<Image>().sprite = heroSO.PortraitInfo;

        _heroInfo.SetActive(true);
        _skillListPanel.SetActive(true);
    }
    /// <summary>
    /// 영웅 상태창 + 스킬리스트 + 선택스킬 정보창 끄기
    /// </summary>
    public void CloseHeroInfomation()
    {
        _heroInfo.SetActive(false);
        _skillListPanel.SetActive(false);
        _cursorSkillInfo.SetActive(false);
        _cursorSkill.SetActive(false);
        _cursorDiamond.SetActive(false);
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
    /// <summary>
    /// 적 정보창 끄기
    /// </summary>
    public void CloseEnemyInfomation()
    {
        _enemyInfo.SetActive(false);
        _enemyResistStatus.SetActive(false);
    }
}
