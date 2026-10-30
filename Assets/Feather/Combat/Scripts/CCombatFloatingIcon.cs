using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CCombatFloatingIcon : MonoBehaviour
{
    #region Enum
    public enum IconType
    {
        None,
        DeathStemp,
        Stress,
        Virtue,
        Turn,
    }
    #endregion

    #region Inspector
    [Header("Icon Inspector")]
    [SerializeField] private IconType _iconType = IconType.None;
    [SerializeField] private float _durationTime = 2f;
    [SerializeField] private float _sharpness = 25f;
    #endregion

    #region Field
    private RectTransform _rectTr;
    private float _deleteTime = 0f;
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

        Vector3 screenPos = Vector3.zero;

        switch (_iconType)
        {
            // 죽음의 일격 아이콘
            case IconType.DeathStemp:
                _rectTr.localScale = Vector3.one * 5f;
                screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
                break;
            // 스트레스 아이콘
            case IconType.Stress:

                break;
            // 스트레스 회복 아이콘
            case IconType.Virtue:

                break;
            // 현재 차례 아이콘 표시
            case IconType.Turn:
                screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
                break;
            default:
                break;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out uiLocalPos);
        _rectTr.anchoredPosition = uiLocalPos;
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
            default:
                break;
        }

    }
}
