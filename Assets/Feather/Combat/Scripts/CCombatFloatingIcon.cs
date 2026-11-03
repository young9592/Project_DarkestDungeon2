using UnityEngine;

public class CCombatFloatingIcon : MonoBehaviour
{
    #region Enum
    public enum IconType
    {
        None,
        DeathStemp,
        Stress,
        StressHealing,
        Turn,
    }
    #endregion

    #region Inspector
    // 아이콘마다 다릅니다.
    [Header("Sub Connect")]
    [SerializeField] private GameObject _subEffect;

    [Header("Option")]
    [SerializeField] private IconType _iconType = IconType.None;
    [SerializeField] private float _durationTime = 2f;
    [SerializeField] private float _sharpness = 25f;
    [SerializeField] private Vector2 _offset = Vector2.zero;
    #endregion

    #region Field
    private RectTransform _rectTr;
    private float _deleteTime = 0f;
    private bool _init = false;
    private bool _rendererCameraMode = false;
    #endregion

    #region Property
    public float DurationTime => _durationTime;
    #endregion

    private void Awake()
    {
        _rectTr = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    // 아이콘 UI 표시
    public void SetIcon(Canvas canvas, GameObject target)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 uiLocalPos;

        // 캔버스 랜더러 모드 체크
        if (!_init)
        {
            if(canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _rendererCameraMode = true;
            }
        }

        Vector3 screenPos = Vector3.zero;


        SkinnedMeshRenderer mesh = target.GetComponentInChildren<SkinnedMeshRenderer>();


        switch (_iconType)
        {
            // 죽음의 일격 아이콘
            case IconType.DeathStemp:
                _rectTr.localScale = Vector3.one * 5f;
                screenPos = Camera.main.WorldToScreenPoint(mesh.bounds.center);
                break;
            // 스트레스 아이콘
            case IconType.Stress:
                _rectTr.localScale = Vector3.one * 5f;
                Vector2 topVec = new Vector2(mesh.bounds.center.x, mesh.bounds.max.y);
                screenPos = Camera.main.WorldToScreenPoint(topVec);
                break;
            // 스트레스 회복 아이콘
            case IconType.StressHealing:
                topVec = new Vector2(mesh.bounds.center.x, mesh.bounds.max.y);
                screenPos = Camera.main.WorldToScreenPoint(topVec);
                break;
            // 현재 차례 아이콘 표시
            case IconType.Turn:
                Vector2 bottomVec = new Vector2(mesh.bounds.center.x, mesh.bounds.min.y);
                screenPos = Camera.main.WorldToScreenPoint(bottomVec);
                break;
            default:
                break;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, _rendererCameraMode ? Camera.main : null, out uiLocalPos);
        _rectTr.anchoredPosition = uiLocalPos + _offset;
        _deleteTime = Time.time + _durationTime;
        gameObject.SetActive(true);

        if (_durationTime != -1)
        {
            Destroy(gameObject, _durationTime);
        }
    }

    void Update()
    {
        ShowIcon();
    }

    private void ShowIcon()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        switch (_iconType)
        {
            case IconType.DeathStemp:
                _rectTr.localScale = Vector3.Lerp(_rectTr.localScale, Vector3.one, 1f - Mathf.Exp(-_sharpness * Time.deltaTime));
                break;
            case IconType.Stress:
                _rectTr.localScale = Vector3.Lerp(_rectTr.localScale, Vector3.one, 1f - Mathf.Exp(-_sharpness * Time.deltaTime));

                if (_rectTr.localScale.x < Vector3.one.x * 1.1f)
                {

                }
                break;
            default:
                break;
        }

    }
}
