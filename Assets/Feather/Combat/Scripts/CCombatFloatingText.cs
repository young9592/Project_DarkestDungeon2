using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CCombatFloatingText : MonoBehaviour
{
    // 어떤 형식의 출력 메세지인가
    public enum EMessageType
    {
        None = 0,

        Damage,
        Damage_Crit,
        Damage_Crit_Msg,

        Heal,
        Heal_Crit_Msg,

        Stress,

        Bleed,
        Blight,
        Burn,

        Daze,
        Stun,
        Speed,

        Death_Door,
        Death
    }
    // 메세지 정렬 방식
    private enum ETextType
    {
        Default,
        Dot,
        Token,
    }
    #region Inspector
    [Header("TextPrefab Inspector")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _iconFrameImage;
    [SerializeField] private Image _iconInnerImage;

    [Header("Option")]
    [SerializeField] private float _duration = 2f;
    [SerializeField] private float _speed = 200f;

    [Header("Sprite")]
    [SerializeField] private Sprite _bleedSprite;
    [SerializeField] private Sprite _blightSprite;
    [SerializeField] private Sprite _burnSprite;
    [SerializeField] private Sprite _dazeSprite;
    [SerializeField] private Sprite _stunSprite;
    [SerializeField] private Sprite _healSprite;
    [SerializeField] private Sprite _speedSprite;
    #endregion

    #region Field
    private RectTransform _rectTr;
    private RectTransform _iconRectTr;
    private RectTransform _textRectTr;
    private bool _stopText = false;
    #endregion

    private void Awake()
    {
        _rectTr = GetComponent<RectTransform>();
        _iconRectTr = _iconInnerImage.GetComponent<RectTransform>();
        _textRectTr = _text.gameObject.GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Move();
    }

    // 상호작용시 표시되는 텍스트
    // offset은 여러 메세지가 한번에 출력되는 경우 위치 조정되는걸 구현하기 위함
    public void SetText(Canvas canvas, GameObject target, EMessageType type, string text, Vector2 offset = default)
    {
        if (canvas == null)
        {
            CPrint.Warn("FloatingText Canvas Disconncet.");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        Vector2 uiLocalPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out uiLocalPos);

        _text.text = text;
        _rectTr.anchoredPosition = uiLocalPos;

        // 다중 메세지 겹침 위치조정
        if (offset != default)
        {
            _rectTr.anchoredPosition += offset;
        }

        switch (type)
        {
            case EMessageType.None:
                SetAlignment(ETextType.Default);
                break;
            // 죽음의 일격!
            case EMessageType.Death:
                SetAlignment(ETextType.Default);
                _text.color = new Color32(213, 68, 17, 255);
                _stopText = true;
                // 죽음의 일격! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 100f;
                break;
            // 데미지, 치명타데미지, 치명타! 메세지
            case EMessageType.Damage:
                SetAlignment(ETextType.Default);
                _speed = 100;
                _text.color = new Color32(218, 20, 37, 255);
                break;
            case EMessageType.Damage_Crit:
                SetAlignment(ETextType.Default);
                _speed = 100;
                _text.color = new Color32(241, 116, 9, 255);
                break;
            case EMessageType.Damage_Crit_Msg:
                SetAlignment(ETextType.Default);
                _speed = 100;
                _text.color = new Color32(241, 116, 9, 255);
                // 치명타! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 50f;
                break;
            case EMessageType.Heal:
                SetAlignment(ETextType.Dot, text);
                _speed = 100;
                _iconImage.sprite = _healSprite;
                _text.color = new Color32(151, 255, 183, 255);
                break;
            case EMessageType.Heal_Crit_Msg:
                SetAlignment(ETextType.Default, text);
                _speed = 100;
                _text.color = new Color32(151, 255, 183, 255);
                // 치명타! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 50f;
                break;
            case EMessageType.Bleed:
                SetAlignment(ETextType.Dot, text);
                _iconImage.sprite = _bleedSprite;
                _text.color = new Color32(140, 8, 24, 255);
                break;
            case EMessageType.Blight:
                _iconImage.sprite = _blightSprite;
                SetAlignment(ETextType.Dot, text);
                _text.color = new Color32(43, 82, 60, 255);
                break;
            case EMessageType.Burn:
                _iconImage.sprite = _burnSprite;
                SetAlignment(ETextType.Dot, text);
                _text.color = new Color32(159, 64, 30, 255);
                break;
            case EMessageType.Stun:
                _iconImage.sprite = _stunSprite;
                SetAlignment(ETextType.Token, text);
                break;
            case EMessageType.Daze:
                _iconImage.sprite = _dazeSprite;
                SetAlignment(ETextType.Token, text);
                break;
            case EMessageType.Speed:
                _iconImage.sprite = _speedSprite;
                SetAlignment(ETextType.Token, text);
                break;
            default:
                break;

        }
        gameObject.SetActive(true);
        Destroy(gameObject, _duration);
    }

    private void SetAlignment(ETextType textType, string text = null)
    {
        switch (textType)
        {
            case ETextType.Default:
                _textRectTr.anchorMin = new Vector2(0.5f, 0.5f);
                _textRectTr.anchorMax = new Vector2(0.5f, 0.5f);
                _textRectTr.pivot = new Vector2(0.5f, 0.5f);
                _textRectTr.anchoredPosition = new Vector2(0, 0);
                _iconImage.enabled = false;
                _iconFrameImage.enabled = false;
                _iconInnerImage.enabled = false;
                break;
            case ETextType.Dot:
                _iconFrameImage.enabled = false;
                _iconInnerImage.enabled = false;
                _text.alignment = TextAlignmentOptions.Left;

                bool isNumber = int.TryParse(text, out int damage);

                if (isNumber)
                {
                    float widthSizeMultiply = text.Length;

                    _textRectTr.sizeDelta = new Vector2(widthSizeMultiply * 30f, 100);
                    _rectTr.sizeDelta = new Vector2(100 + widthSizeMultiply * 30f, 100);
                }
                else
                {
                    _text.margin = new Vector4(30, 0, 0, 0);
                }
                break;
            case ETextType.Token:
                _textRectTr.anchorMin = new Vector2(1, 0.5f);
                _textRectTr.anchorMax = new Vector2(1, 0.5f);
                _textRectTr.pivot = new Vector2(1, 0.5f);
                _textRectTr.anchoredPosition = new Vector2(-100, 0);

                _iconRectTr.anchorMin = new Vector2(1, 0.5f);
                _iconRectTr.anchorMax = new Vector2(1, 0.5f);
                _iconRectTr.pivot = new Vector2(1, 0.5f);
                _iconRectTr.anchoredPosition = new Vector2(0, 0);
                _text.alignment = TextAlignmentOptions.Right;
                _text.margin = new Vector4(0, 0, 30, 0);
                break;
        }
    }

    public void Move()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (_stopText)
        {
            return;
        }

        _rectTr.anchoredPosition += Vector2.up * _speed * Time.deltaTime;
    }
}
