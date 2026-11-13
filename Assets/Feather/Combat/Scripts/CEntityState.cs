using System;
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

    [Header("Entity Health")]
    [SerializeField] private Image _hpFill;
    [SerializeField] private RectTransform _hpBarBackgroundTr;

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
    private GameObject _tokenTooltip;
    private GameObject _dotTooltip;
    #endregion

    #region Property
    public bool DoFollow
    {
        get; set;
    }
    #endregion

    private void Awake()
    {
        // Null check

        _rectTr = GetComponent<RectTransform>();
    }

    // 초기 생성시 연결처리
    public void Initialize(Canvas canvas, GameObject tokenTooltip, GameObject dotTooltip, GameObject target)
    {
        _canvas = canvas;
        _target = target;

        if (_tokenTooltip == null)
        {
            _tokenTooltip = tokenTooltip;
        }

        if(_dotTooltip == null)
        {
            _dotTooltip = dotTooltip;
        }

        if (_canvas == null || _target == null)
        {
            CPrint.Warn($"{gameObject.name} : Initialize 값 전달 오류");
            return;
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
                _hpBarBackgroundTr.localScale = new Vector3(_hpBarBackgroundTr.localScale.x * _formationSize, _hpBarBackgroundTr.localScale.y, _hpBarBackgroundTr.localScale.z);
            }

        }
        else if (isEnemyCorpse)
        {
            _stressRoot.SetActive(false);
            _entityType = EEntityState.Enemy_Corpse;

            _formationSize = (entity as CEnemy).Corpse.FormationSize;

            if (_formationSize > 1)
            {
                _hpBarBackgroundTr.localScale = new Vector3(_hpBarBackgroundTr.localScale.x * _formationSize, _hpBarBackgroundTr.localScale.y, _hpBarBackgroundTr.localScale.z);
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
    // 현재 변화된 상태가 있는지 체크 [ 아래 코드들 SetHealth부터 변화가 있을 지점에만 배치된 함수들입니다. ]
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
        foreach (EEffectType effect in Enum.GetValues(typeof(EEffectType)))
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
        int curHealth = 0;

        switch (_entityType)
        {
            case EEntityState.None:
                break;
            case EEntityState.Hero:
                curHealth = ((CPlayer)entity).CurrentHp;
                break;
            case EEntityState.Enemy:
                curHealth = ((CEnemy)entity).CurrentHp;
                break;
            case EEntityState.Enemy_Corpse:
                curHealth = ((CEnemyCorpse)entity).CurrentHp;
                break;
        }


        float Clamp01 = Mathf.Clamp01((float)curHealth / (float)maxHealth);
        _hpFill.fillAmount = Clamp01;

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

            if (deathArmorCount != enemy.TokenManager.GetTokenStack(EEffectType.DeathArmor))
            {
                enemy.TokenManager.AddToken(EEffectType.DeathArmor, deathArmorCount, 99);
            }
            else if (enemy.IsDeathsDoor && deathArmorCount == 0 && enemy.TokenManager.HasToken(EEffectType.DeathDoor))
            {
                enemy.TokenManager.AddToken(EEffectType.DeathDoor, 1, 99);
            }
            else if (!enemy.IsDeathsDoor && enemy.TokenManager.HasToken(EEffectType.DeathDoor))
            {
                enemy.TokenManager.ConsumeToken(EEffectType.DeathDoor);
            }
        }
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
    }
}
