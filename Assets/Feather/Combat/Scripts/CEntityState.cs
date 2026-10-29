using UnityEngine;
using UnityEngine.UI;


// 현재 엔티티 상태
public class CEntityState : MonoBehaviour
{
    #region Inspector
    [Header("Connect Object")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _entity;

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
    private MeshRenderer _mesh;
    private RectTransform _canvasRectTr;
    // 초기 연결상태 확인
    private bool _initEntity = false;
    private bool _doFollow = false;
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
    public void Initialize(Canvas canvas, GameObject entity)
    {
        _canvas = canvas;
        _entity = entity;

        if (_canvas == null || _entity == null)
        {
            CPrint.Warn($"{gameObject.name} : Initialize 값 전달 오류");
            return;
        }

        CTempEntity entityScript = _entity.GetComponent<CTempEntity>();

        _mesh = _entity.GetComponent<MeshRenderer>();
        _canvasRectTr = _canvas.GetComponent<RectTransform>();

        SetHealth();

        if (entityScript.Type == EEntityType.Enemy)
        {
            _stressRoot.SetActive(false);
        }
        else
        {
            SetStress();
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
        Vector3 targetVec = _entity.transform.position;
        Vector3 bottomPos = new Vector3(targetVec.x, _mesh.bounds.min.y, targetVec.z);
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
        if (_entity == null)
        {
            Destroy(gameObject);
        }
    }
    public void SetHealth()
    {
        CTempEntity entity = _entity.GetComponent<CTempEntity>();

        int maxHealth = entity.EntityData.MaxHealth;
        int curHealth = entity.CurrentHealth;

        float Clamp01 = Mathf.Clamp01((float)curHealth / (float)maxHealth);
        _HealthFill.fillAmount = Clamp01;
    }
    public void SetStress()
    {
        CTempEntity entity = _entity.GetComponent<CTempEntity>();

        // 적은 스트레스가 없다.
        if (entity.Type == EEntityType.Enemy)
        {
            return;
        }

        int maxStress = entity.MaxStress;
        int curStress = entity.CurrentStress;

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
