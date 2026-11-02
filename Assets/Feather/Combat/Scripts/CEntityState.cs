using System;
using UnityEngine;
using UnityEngine.UI;


// 현재 엔티티 상태
public class CEntityState : MonoBehaviour
{
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

    [Header("Entity Health")]
    [SerializeField] private Image _HealthFill;

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
    [SerializeField] private GameObject _horror;
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
    private RectTransform _rectTr;
    private RectTransform _canvasRectTr;
    // 초기 연결상태 확인
    private bool _initEntity = false;
    private bool _doFollow = false;
    private bool _isHero = false;
    #endregion

    #region Property
    public bool DoFollow
    {
        get; set;
    }
    #endregion

    private void Awake()
    {
        _rectTr = GetComponent<RectTransform>();
    }

    // 초기 생성시 연결처리
    public void Initialize(Canvas canvas, GameObject target)
    {
        _canvas = canvas;
        _target = target;

        if (_canvas == null || _target == null)
        {
            CPrint.Warn($"{gameObject.name} : Initialize 값 전달 오류");
            return;
        }

        CEntity entity = _target.GetComponent<CEntity>();

        _canvasRectTr = _canvas.GetComponent<RectTransform>();


        bool isHero = entity is CPlayer;

        _isHero = isHero;

        SetHealth();

        if (isHero)
        {
            SetStress();
        }
        else
        {
            _stressRoot.SetActive(false);
        }

        MoveState();


        _initEntity = true;
    }

    private void Update()
    {
        if (!_initEntity)
        {
            return;
        }

        CheckEntityDead();
        FollowEntity();
        // 원래는 부분지점에서 처리하고 있었으나 부분별로 처리되는 곳이 있어서 추가
        SetTotalState();
    }

    // 추적이동
    private void FollowEntity()
    {
        if (!_doFollow)
        {
            return;
        }

        MoveState();
    }

    // 이동
    private void MoveState()
    {
        Vector3 targetVec = _target.transform.position;
        Vector3 bottomPos = _target.transform.position + _offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(bottomPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTr, screenPos, null, out Vector2 uiLocalPos);

        _rectTr.anchoredPosition = uiLocalPos - new Vector2(0, _rectTr.sizeDelta.y * 0.5f);
    }

    private void CheckEntityDead()
    {
        if (!_initEntity)
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
            if (effect == EEffectType.Bleed || effect == EEffectType.Blight || effect == EEffectType.Burn)
            {
                if (_isHero)
                {
                    CPlayer hero = _target.GetComponent<CPlayer>();
                    int haveDamage = hero.GetTotalDotDamageByType(effect);
                    bool remainDot = (haveDamage > 0 ? true : false);
                    SetEffect(effect, remainDot);
                }
                else
                {
                    CEnemy enemy = _target.GetComponent<CEnemy>();
                    int haveDamage = enemy.GetTotalDotDamageByType(effect);
                    bool remainDot = (haveDamage > 0 ? true : false);
                    SetEffect(effect, remainDot);
                }
            }
            else
            {
                CEntity entity = _target.GetComponent<CEntity>();
                SetEffect(effect, entity.TokenManager.HasToken(effect));
            }
        }
    }

    public void SetHealth()
    {
        CEntity entity = _target.GetComponent<CEntity>();

        int maxHealth = entity.EntityData.MaxHealth;
        int curHealth = _isHero ? ((CPlayer)entity).CurrentHp : ((CEnemy)entity).CurrentHp;

        float Clamp01 = Mathf.Clamp01((float)curHealth / (float)maxHealth);
        _HealthFill.fillAmount = Clamp01;
    }
    public void SetStress()
    {
        CEntity entity = _target.GetComponent<CEntity>();

        // 적은 스트레스가 없다.
        if (!_isHero)
        {
            return;
        }

        int maxStress = 10;
        int curStress = ((CPlayer)entity).CurrentStress;

        float Clamp01 = Mathf.Clamp01((float)curStress / (float)maxStress);
        _stressFill.fillAmount = Clamp01;
    }

    public void SetEffect(EEffectType effectType, bool toggle)
    {
        switch (effectType)
        {
            case EEffectType.Bleed:
                _bleed.SetActive(toggle);
                break;
            case EEffectType.Blight:
                _blight.SetActive(toggle);
                break;
            case EEffectType.Burn:
                _burn.SetActive(toggle);
                break;
            case EEffectType.Crit:
                _crit.SetActive(toggle);
                break;
            case EEffectType.Riposte:
                _riposte.SetActive(toggle);
                break;
            case EEffectType.Dodge:
                _dodge.SetActive(toggle);
                break;
            case EEffectType.DodgePlus:
                _dodgePlus.SetActive(toggle);
                break;
            case EEffectType.Block:
                _block.SetActive(toggle);
                break;
            case EEffectType.BlockPlus:
                _blockPlus.SetActive(toggle);
                break;
            case EEffectType.Guarded:
                _guard.SetActive(toggle);
                break;
            case EEffectType.Strength:
                _strength.SetActive(toggle);
                break;
            case EEffectType.Speed:
                _speed.SetActive(toggle);
                break;
            case EEffectType.Stealth:
                _stealth.SetActive(toggle);
                break;
            case EEffectType.Daze:
                _daze.SetActive(toggle);
                break;
            case EEffectType.Stun:
                _stun.SetActive(toggle);
                break;
            case EEffectType.Blind:
                _blind.SetActive(toggle);
                break;
            case EEffectType.Weak:
                _weak.SetActive(toggle);
                break;
            case EEffectType.Vulnerable:
                _vulnerable.SetActive(toggle);
                break;
            case EEffectType.Taunt:
                _taunt.SetActive(toggle);
                break;
            case EEffectType.Immobilize:
                _immoblize.SetActive(toggle);
                break;
            case EEffectType.Combo:
                _combo.SetActive(toggle);
                break;
            default:
                break;
        }
    }
}
