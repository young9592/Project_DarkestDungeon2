using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// 현재 엔티티 상태
public class CEntityState : MonoBehaviour
{
    public enum EEntityState
    {
        None,
        Hero,
        Enemy,
        Enemy_Corpse
    }
    #region Inspector
    [Header("Connect Object")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _target;

    [Header("Option")]
    [SerializeField] private Vector3 _offset = new Vector3(0, -1, 0);

    [Header("Entity Dot")]
    [SerializeField] private GameObject _bleed;
    [SerializeField] private GameObject _blight;
    [SerializeField] private GameObject _burn;
    [SerializeField] private GameObject _horror;
    [SerializeField] private GameObject _regen;

    [Header("HealthInfomation Detect Area")]
    [SerializeField] private CHealthInfomation _healthInfomation;

    [Header("Entity Health")]
    [SerializeField] private RectTransform _hpbarRoot;
    [SerializeField] private Image _hpFill;
    [SerializeField] private Image _hpLost;
    [SerializeField] private Image _hpRecover;

    [Header("Entity Stress")]
    [SerializeField] private GameObject _stressRoot;
    [SerializeField] private Image _stressFill;

    [Header("Entity Token")]
    [SerializeField] private GameObject _daze;
    [SerializeField] private GameObject _stun;
    [SerializeField] private GameObject _taunt;
    [SerializeField] private GameObject _vulnerable;
    [SerializeField] private GameObject _weak;
    [SerializeField] private GameObject _blind;
    [SerializeField] private GameObject _immoblize;
    [SerializeField] private GameObject _combo;
    [SerializeField] private GameObject _death;
    [SerializeField] private GameObject _deathArmor;
    [SerializeField] private GameObject _block;
    [SerializeField] private GameObject _blockPlus;
    [SerializeField] private GameObject _guard;
    [SerializeField] private GameObject _dodge;
    [SerializeField] private GameObject _dodgePlus;
    [SerializeField] private GameObject _speed;
    [SerializeField] private GameObject _strength;
    [SerializeField] private GameObject _stealth;
    [SerializeField] private GameObject _riposte;
    [SerializeField] private GameObject _crit;
    #endregion

    #region Field
    private Dictionary<EEffectType, GameObject> _effectDict = new Dictionary<EEffectType, GameObject>();
    private RectTransform _rectTr;
    private RectTransform _canvasRectTr;

    // 초기 연결상태 확인
    private bool _init = false;
    private bool _doFollow = true;
    private int _formationSize = 1;
    private EEntityState _entityType = EEntityState.None;
    private bool _rendererCameraMode = false;
    // 역동적인 체력바 구현
    private int _prevHealth = -1;
    private Coroutine _effectHpBarCoroutine = null;
    // 중간에 변경되는 사항 적용
    private bool _againCoroutine = false;
    private float _changeDesire = 0f;
    //툴팁
    private static GameObject _tokenTooltip;
    private static GameObject _dotTooltip;
    private static GameObject _healthTooltip;
    private List<Object> _checkList = new List<Object>();
    #endregion

    #region Property
    public bool DoFollow
    {
        get; set;
    }
    #endregion

    private void Awake()
    {
        // Effect
        _checkList.Add(_bleed);
        _checkList.Add(_blight);
        _checkList.Add(_burn);
        _checkList.Add(_horror);
        _checkList.Add(_regen);

        // Entity Health
        _checkList.Add(_hpbarRoot);
        _checkList.Add(_hpFill);
        _checkList.Add(_hpLost);
        _checkList.Add(_hpRecover);

        // Entity Stress
        _checkList.Add(_stressRoot);
        _checkList.Add(_stressFill);

        // Entity Token
        _checkList.Add(_daze);
        _checkList.Add(_stun);
        _checkList.Add(_taunt);
        _checkList.Add(_vulnerable);
        _checkList.Add(_weak);
        _checkList.Add(_blind);
        _checkList.Add(_immoblize);
        _checkList.Add(_combo);
        _checkList.Add(_death);
        _checkList.Add(_deathArmor);
        _checkList.Add(_block);
        _checkList.Add(_blockPlus);
        _checkList.Add(_guard);
        _checkList.Add(_dodge);
        _checkList.Add(_dodgePlus);
        _checkList.Add(_speed);
        _checkList.Add(_strength);
        _checkList.Add(_stealth);
        _checkList.Add(_riposte);
        _checkList.Add(_crit);

        _checkList.Add(_healthInfomation);

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
        {
            enabled = false;
            return;
        }

        _rectTr = GetComponent<RectTransform>();

        _hpLost.enabled = false;
        _hpRecover.enabled = false;
    }

    // 초기 생성시 연결처리
    public void Initialize(Canvas canvas, GameObject healthTooltip, GameObject tokenTooltip, GameObject dotTooltip, GameObject target)
    {
        _canvas = canvas;
        _target = target;

        if (_canvas == null || _target == null)
        {
            CPrint.Warn($"{gameObject.name} : Initialize 값 전달 오류");
            return;
        }

        // 툴팁 연결
        if (_healthTooltip == null)
        {
            _healthTooltip = healthTooltip;
        }

        if (_tokenTooltip == null)
        {
            _tokenTooltip = tokenTooltip;
        }

        if (_dotTooltip == null)
        {
            _dotTooltip = dotTooltip;
        }

        // 랜더링 모드 체크
        if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            _rendererCameraMode = true;
        }

        CEntity entity = _target.GetComponent<CEntity>();

        _canvasRectTr = _canvas.GetComponent<RectTransform>();

        bool isHero = entity is CPlayer;
        bool isEnemy = entity is CEnemy;
        bool isEnemyCorpse = entity is CEnemyCorpse;

        SetHealth();

        if (isHero)
        {
            SetStress();
            _entityType = EEntityState.Hero;
            _formationSize = 1;
        }
        else if (isEnemy)
        {
            _stressRoot.SetActive(false);
            _entityType = EEntityState.Enemy;

            _formationSize = (entity as CEnemy).EnemyData.FormationSize;

            if (_formationSize > 1)
            {
                _hpbarRoot.sizeDelta = new Vector2(_hpbarRoot.sizeDelta.x * _formationSize, _hpbarRoot.sizeDelta.y);
            }

        }
        else if (isEnemyCorpse)
        {
            _stressRoot.SetActive(false);
            _entityType = EEntityState.Enemy_Corpse;

            _formationSize = (entity as CEnemy).Corpse.FormationSize;

            if (_formationSize > 1)
            {
                _hpbarRoot.sizeDelta = new Vector2(_hpbarRoot.sizeDelta.x * _formationSize, _hpbarRoot.sizeDelta.y);
            }
        }

        _effectDict.Add(EEffectType.Bleed, _bleed);
        _effectDict.Add(EEffectType.Blight, _blight);
        _effectDict.Add(EEffectType.Burn, _burn);
        _effectDict.Add(EEffectType.Horror, _horror);
        _effectDict.Add(EEffectType.Regen, _regen);
        _effectDict.Add(EEffectType.Daze, _daze);
        _effectDict.Add(EEffectType.Stun, _stun);
        _effectDict.Add(EEffectType.Taunt, _taunt);
        _effectDict.Add(EEffectType.Vulnerable, _vulnerable);
        _effectDict.Add(EEffectType.Weak, _weak);
        _effectDict.Add(EEffectType.Blind, _blind);
        _effectDict.Add(EEffectType.Immobilize, _immoblize);
        _effectDict.Add(EEffectType.Combo, _combo);
        _effectDict.Add(EEffectType.DeathDoor, _death);
        _effectDict.Add(EEffectType.DeathArmor, _deathArmor);
        _effectDict.Add(EEffectType.Block, _block);
        _effectDict.Add(EEffectType.BlockPlus, _blockPlus);
        _effectDict.Add(EEffectType.Guard, _guard);
        _effectDict.Add(EEffectType.Dodge, _dodge);
        _effectDict.Add(EEffectType.DodgePlus, _dodgePlus);
        _effectDict.Add(EEffectType.Speed, _speed);
        _effectDict.Add(EEffectType.Strength, _strength);
        _effectDict.Add(EEffectType.Stealth, _stealth);
        _effectDict.Add(EEffectType.Riposte, _riposte);
        _effectDict.Add(EEffectType.Crit, _crit);

        MoveState();

        if (!_healthInfomation.IsInitialize && _entityType != EEntityState.None)
        {
            _healthInfomation.Initialize(_healthTooltip, _entityType);
        }

        _init = true;
    }
    // 시체 상태가 되었을 때 재연결
    public void ReconnectState(GameObject target)
    {
        _init = false;

        // 랜더링 모드 체크
        if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            _rendererCameraMode = true;
        }

        if (target.TryGetComponent<CEnemyCorpse>(out CEnemyCorpse enemyCorpse))
        {
            _entityType = EEntityState.Enemy_Corpse;
            _target = target;
        }

        _prevHealth = -1;
        SetTotalState();

        _init = true;
    }
    private void Update()
    {
        if (!_init)
        {
            return;
        }

        CheckEntityDead();
        FollowEntity();
        SetTotalState();
    }

    // 추적이동
    private void FollowEntity()
    {
        if (!_doFollow)
        {
            return;
        }

        if (_target == null)
        {
            return;
        }

        MoveState();
    }

    // 이동
    private void MoveState()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 targetVec = _target.transform.position;
        Vector3 bottomPos = _target.transform.position + _offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(bottomPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTr, screenPos, _rendererCameraMode ? Camera.main : null, out Vector2 uiLocalPos);

        _rectTr.anchoredPosition = uiLocalPos - new Vector2(0, _rectTr.sizeDelta.y * 0.5f);
    }

    private void CheckEntityDead()
    {
        if (!_init)
        {
            return;
        }

        // 엔티티가 연결이 끊겼다.
        if (_target == null)
        {
            Destroy(gameObject);
        }
    }
    // 현재 변화된 상태가 있는지 체크
    public void SetTotalState()
    {
        if (_target == null)
        {
            return;
        }

        SetHealth();
        SetStress();
        CheckEffect();
    }
    // 플레이어 내부에서도 작업되는게 있어서 추가된 함수
    private void CheckEffect()
    {
        foreach (EEffectType effect in System.Enum.GetValues(typeof(EEffectType)))
        {
            // 도트 데미지
            if (effect == EEffectType.Bleed || effect == EEffectType.Blight || effect == EEffectType.Burn || effect == EEffectType.Horror || effect == EEffectType.Regen)
            {
                if (_effectDict.TryGetValue(effect, out GameObject target))
                {
                    CDotInfomation dotInfomation = target.GetComponent<CDotInfomation>();

                    if (!dotInfomation.IsInitialize)
                    {
                        dotInfomation.Initialize(_dotTooltip);
                    }

                    if (_entityType == EEntityState.Hero)
                    {
                        CPlayer hero = _target.GetComponent<CPlayer>();
                        int totalDamage = hero.GetTotalDotDamageByType(effect);
                        int remain = hero.GetMaxRemainingEffectTurns(effect);
                        bool remainDot = (totalDamage > 0 ? true : false);

                        if (remainDot)
                        {
                            if (!target.activeSelf)
                            {
                                target.SetActive(true);
                            }

                            dotInfomation.SetData(remain, totalDamage);
                        }
                        else
                        {
                            dotInfomation.ResetDot();
                        }
                    }
                    else if (_entityType == EEntityState.Enemy)
                    {
                        CEnemy enemy = _target.GetComponent<CEnemy>();
                        int totalDamage = enemy.GetTotalDotDamageByType(effect);
                        int remain = enemy.GetMaxRemainingEffectTurns(effect);
                        bool remainDot = (totalDamage > 0 ? true : false);

                        if (remainDot)
                        {
                            if (!target.activeSelf)
                            {
                                target.SetActive(true);
                            }

                            dotInfomation.SetData(remain, totalDamage);
                        }
                        else
                        {
                            dotInfomation.ResetDot();
                        }

                    }
                    // 시체는 현재 효과를 받을 수 없기 때문에 모두 꺼주기
                    else if (_entityType == EEntityState.Enemy_Corpse)
                    {
                        dotInfomation.ResetDot();
                    }
                }
            }
            // 토큰
            else
            {
                if (_effectDict.TryGetValue(effect, out GameObject target))
                {
                    CTokenInfomation tokenInfomation = target.GetComponent<CTokenInfomation>();

                    if (!tokenInfomation.IsInitialize)
                    {
                        tokenInfomation.Initialize(_tokenTooltip);
                    }

                    if (_entityType == EEntityState.Enemy_Corpse)
                    {
                        tokenInfomation.ResetToken();
                        continue;
                    }

                    CEntity entity = _target.GetComponent<CEntity>();
                    CTokenManager tokenManager = entity.TokenManager;

                    if (tokenManager.HasToken(effect))
                    {
                        int duration = tokenManager.GetTokenDuration(effect);
                        int stack = tokenManager.GetTokenStack(effect);

                        if (!target.activeSelf)
                        {
                            target.SetActive(true);
                        }

                        tokenInfomation.SetTokenState(duration, stack);
                    }
                    else
                    {
                        tokenInfomation.ResetToken();
                    }
                }
            }
        }
    }

    private void SetHealth()
    {
        if (_target == null)
        {
            return;
        }

        CEntity entity = _target.GetComponent<CEntity>();

        int maxHealth = entity.EntityData.MaxHealth;
        int curHealth = -1;



        switch (_entityType)
        {
            case EEntityState.Hero:
                CPlayer hero = entity as CPlayer;
                curHealth = hero.CurrentHp;
                break;
            case EEntityState.Enemy:
                CEnemy enemy = entity as CEnemy;
                curHealth = enemy.CurrentHp;
                break;
            case EEntityState.Enemy_Corpse:
                CEnemyCorpse enemyCorpse = entity as CEnemyCorpse;
                curHealth = enemyCorpse.CurrentHp;
                break;
        }

        // 변화된 체력
        if (curHealth == -1)
        {
            _hpFill.fillAmount = 1;
        }
        else if (_prevHealth == -1)
        {
            _hpFill.fillAmount = 1;
        }
        // 데미지를 입은 상황
        else if (_prevHealth > curHealth)
        {
            // 깍인만큼 바로 적용
            float currentHpClamp = Mathf.Clamp01((float)curHealth / (float)maxHealth);
            _hpFill.fillAmount = currentHpClamp;

            // 체력을 잃기 전 위치에서부터..
            float changeHpClame = Mathf.Clamp01((float)_prevHealth / (float)maxHealth);
            _hpLost.enabled = true;
            _hpLost.fillAmount = changeHpClame;

            // 잃은 만큼 보간으로 애니메이션 적용
            if (_effectHpBarCoroutine == null)
            {
                _effectHpBarCoroutine = StartCoroutine(CoHealthChangeEffect(_hpLost, changeHpClame, currentHpClamp, _hpLost));
            }
            else
            {
                _changeDesire = Mathf.Clamp01((float)curHealth / (float)maxHealth);
                _againCoroutine = true;
            }
        }
        // 체력 회복한 상황
        else if (_prevHealth < curHealth)
        {
            // 현재 체력은 변화되기 전 위치에서
            float currentHpClamp = Mathf.Clamp01((float)_prevHealth / (float)maxHealth);
            _hpFill.fillAmount = currentHpClamp;

            // 회복하는 양 위치 적용
            float changeHpClame = Mathf.Clamp01((float)curHealth / (float)maxHealth);
            _hpRecover.enabled = true;
            _hpRecover.fillAmount = changeHpClame;

            // 회복된 만큼 애니메이션 적용
            if (_effectHpBarCoroutine == null)
            {
                _effectHpBarCoroutine = StartCoroutine(CoHealthChangeEffect(_hpFill, currentHpClamp, changeHpClame, _hpRecover));
            }
            else
            {
                _changeDesire = Mathf.Clamp01((float)curHealth / (float)maxHealth);
                _againCoroutine = true;
            }
        }

        _prevHealth = curHealth;

        // 죽음의 문턱 상태인지 확인
        if (entity is CPlayer)
        {
            if (entity.IsDeathsDoor && !entity.TokenManager.HasToken(EEffectType.DeathDoor))
            {
                entity.TokenManager.AddToken(EEffectType.DeathDoor, 1, 99);
            }
            else if (!entity.IsDeathsDoor && entity.TokenManager.HasToken(EEffectType.DeathDoor))
            {
                entity.TokenManager.ConsumeToken(EEffectType.DeathDoor);
            }
        }
        else if (entity is CEnemy)
        {
            CEnemy enemy = entity as CEnemy;
            int deathArmorCount = enemy.RemainingDeathArmor;


            if (deathArmorCount == 0 && enemy.TokenManager.HasToken(EEffectType.DeathArmor))
            {
                enemy.TokenManager.ConsumeToken(EEffectType.DeathArmor);
                enemy.TokenManager.AddToken(EEffectType.DeathDoor);
            }
            else if (deathArmorCount > 0 && deathArmorCount != enemy.TokenManager.GetTokenStack(EEffectType.DeathArmor))
            {
                enemy.TokenManager.AddToken(EEffectType.DeathArmor, deathArmorCount, 99);
            }
        }

        // 데이터 적용
        _healthInfomation.SetHealth(curHealth, maxHealth);
    }
    // 회복이나 데미지를 받을 시 체력 깍이는 이펙트 [ 만약 깍이거나 회복되는 이벤트가 동시에 들어오면 애니메이션 오류가 생길겁니다. ]
    private IEnumerator CoHealthChangeEffect(Image target, float start, float desire, Image effect)
    {
        float delay = 1f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            // 중간에 회복이 또 들어오는 경우 [ 주로 디버그 ]
            if (_againCoroutine)
            {
                desire = _changeDesire;
                _againCoroutine = false;
                _changeDesire = 0;
            }

            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);

            target.fillAmount = Mathf.Lerp(start, desire, t);

            yield return null;
        }

        target.fillAmount = desire;
        effect.enabled = false;
        _againCoroutine = false;
        _changeDesire = 0;
        _effectHpBarCoroutine = null;
    }
    private void SetStress()
    {
        CEntity entity = _target.GetComponent<CEntity>();

        // 적은 스트레스가 없다.
        if (_entityType == EEntityState.Enemy)
        {
            return;
        }

        if (_entityType == EEntityState.Enemy_Corpse)
        {
            return;
        }

        int maxStress = 10;
        int curStress = ((CPlayer)entity).CurrentStress;

        float Clamp01 = Mathf.Clamp01((float)curStress / (float)maxStress);
        _stressFill.fillAmount = Clamp01;

        _healthInfomation.SetStress(curStress, maxStress);
    }
}
