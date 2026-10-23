using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CIcon : MonoBehaviour
{
    #region Enum
    public enum IconType
    {
        None,
        DeathStemp,
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

    public void SetIcon(Canvas canvas, GameObject target)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 uiLocalPos;

        Vector3 screenPos = Vector3.zero;

        switch (_iconType)
        {
            case IconType.DeathStemp:
                _rectTr.localScale = Vector3.one * 5f;
                screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
                break;
            case IconType.Turn:
                MeshRenderer mesh = target.GetComponent<MeshRenderer>();
                // 현재 차례의 오브젝트 아랫부분
                Vector3 targetVec = target.transform.position;
                Vector3 bottomPos = new Vector3(targetVec.x, mesh.bounds.min.y, targetVec.z);

                screenPos = Camera.main.WorldToScreenPoint(bottomPos);
                break;
            default:
                break;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out uiLocalPos);
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
