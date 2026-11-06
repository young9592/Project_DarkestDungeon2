using System;
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
    private bool _doFollow = true;
    private EEntityState _entityType = EEntityState.None;
    private bool _rendererCameraMode = false;
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
        }
        else if (isEnemy)
        {
            _stressRoot.SetActive(false);
            _entityType = EEntityState.Enemy;
        }
        else if (isEnemyCorpse)
        {
            _stressRoot.SetActive(false);
            _entityType = EEntityState.Enemy_Corpse;
        }

        MoveState();

        _initEntity = true;
    }
    // 시체 상태가 되었을 때 재연결
    public void ReConnect(GameObject target)
    {
        _initEntity = false;

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
            if (effect == EEffectType.Bleed || effect == EEffectType.Blight || effect == EEffectType.Burn || effect == EEffectType.Stress)
            {
                if (_entityType == EEntityState.Hero)
                {
                    CPlayer hero = _target.GetComponent<CPlayer>();
                    int haveDamage = hero.GetTotalDotDamageByType(effect);
                    bool remainDot = (haveDamage > 0 ? true : false);
                    SetEffect(effect, remainDot);
                }
                else if (_entityType == EEntityState.Enemy)
                {
                    CEnemy enemy = _target.GetComponent<CEnemy>();
                    int haveDamage = enemy.GetTotalDotDamageByType(effect);
                    bool remainDot = (haveDamage > 0 ? true : false);
                    SetEffect(effect, remainDot);
                }
                // 시체는 현재 효과를 받을 수 없기 때문에 모두 꺼주기
                else if (_entityType == EEntityState.Enemy_Corpse)
                {
                    SetEffect(effect, false);
                }
            }
            else
            {
                if (_entityType == EEntityState.Enemy_Corpse)
                {
                    SetEffect(effect, false);
                }

                CEntity entity = _target.GetComponent<CEntity>();
                SetEffect(effect, entity.TokenManager.HasToken(effect));
            }
        }
    }

    public void SetHealth()
    {
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
        _HealthFill.fillAmount = Clamp01;
    }
    public void SetStress()
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
            case EEffectType.Guard:
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
            case EEffectType.Stress:
                _horror.SetActive(toggle);
                break;
            default:
                break;
        }
    }
}
