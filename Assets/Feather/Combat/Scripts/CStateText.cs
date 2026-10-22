using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CStateText : MonoBehaviour
{
    private enum TextType
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
    [SerializeField] private float _duration = 2f;
    [SerializeField] private float _speed = 200f;

    [Header("Sprite")]
    [SerializeField] private Sprite _bleedSprite;
    [SerializeField] private Sprite _blightSprite;
    [SerializeField] private Sprite _burnSprite;
    [SerializeField] private Sprite _dazeSprite;
    [SerializeField] private Sprite _stunSprite;
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
    // offset은 고려중입니다.
    public void SetText(Canvas canvas, Vector3 screenPos, EEffectType type, string text, Color32 color = default, Vector2 offset = default)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 uiLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out uiLocalPos);

        _text.text = text;
        _rectTr.anchoredPosition = uiLocalPos;

        if (offset != default)
        {
            _rectTr.anchoredPosition += offset;
        }

        switch (type)
        {
            case EEffectType.None:
                SetAlignment(TextType.Default);

                // 임시
                if (text.Equals("죽음의 일격!"))
                {
                    _text.color = new Color32(213, 68, 17, 255);
                    _stopText = true;
                    _rectTr.anchoredPosition += Vector2.up * 100f;
                }
                break;
            case EEffectType.Damage:

                _speed = 100;
                SetAlignment(TextType.Default);

                // 이건 임시입니다.
                // 치명타 데미지가 들어왔을 때 텍스트를 다르게 처리하기 때문에 위와같이 처리하였습니다.
                if (color.Equals(new Color32(241, 116, 9, 255)))
                {
                    _text.color = color;

                    // 숫자가 아니면
                    if (!int.TryParse(text, out int damage))
                    {
                        _rectTr.anchoredPosition += Vector2.up * 50f;
                    }
                }
                else
                {
                    _text.color = new Color32(218, 20, 37, 255);
                }
                break;
            // 후에 수정 필요할수도 있음
            case EEffectType.Heal:
                SetAlignment(TextType.Dot, text);
                _text.color = new Color32(151, 255, 183, 255);
                break;
            case EEffectType.Bleed:
                _iconImage.sprite = _bleedSprite;
                SetAlignment(TextType.Dot, text);
                _text.color = new Color32(140, 8, 24, 255);
                break;
            case EEffectType.Blight:
                _iconImage.sprite = _blightSprite;
                SetAlignment(TextType.Dot, text);
                _text.color = new Color32(43, 82, 60, 255);
                break;
            case EEffectType.Burn:
                _iconImage.sprite = _burnSprite;
                SetAlignment(TextType.Dot, text);
                _text.color = new Color32(159, 64, 30, 255);
                break;
            case EEffectType.Stun:
                _iconImage.sprite = _stunSprite;
                SetAlignment(TextType.Token, text);
                break;
            case EEffectType.Daze:
                _iconImage.sprite = _dazeSprite;
                SetAlignment(TextType.Token, text);
                break;

        }
        gameObject.SetActive(true);
        Destroy(gameObject, _duration);
    }

    private void SetAlignment(TextType textType, string text = null)
    {
        switch (textType)
        {
            case TextType.Default:
                _textRectTr.anchorMin = new Vector2(0.5f, 0.5f);
                _textRectTr.anchorMax = new Vector2(0.5f, 0.5f);
                _textRectTr.pivot = new Vector2(0.5f, 0.5f);
                _textRectTr.anchoredPosition = new Vector2(0, 0);
                _iconImage.enabled = false;
                _iconFrameImage.enabled = false;
                _iconInnerImage.enabled = false;
                break;
            case TextType.Dot:
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
            case TextType.Token:
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
