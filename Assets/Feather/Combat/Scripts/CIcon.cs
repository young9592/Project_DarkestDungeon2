using UnityEngine;

public class CIcon : MonoBehaviour
{
    #region Enum
    public enum IconType
    {
        None,
        DeathStemp,

    }
    #endregion

    #region Inspector
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

    public void SetIcon(Canvas canvas, Vector3 screenPos)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 uiLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out uiLocalPos);

        _rectTr.anchoredPosition = uiLocalPos;
        _deleteTime = Time.time + _durationTime;
        gameObject.SetActive(true);
        Destroy(gameObject, _durationTime);

        switch (_iconType)
        {
            case IconType.DeathStemp:
                _rectTr.localScale = Vector3.one * 5f;
                break;
            default:
                break;
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
