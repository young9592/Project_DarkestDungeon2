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
        Damage_Critical,
        Damage_Critical_Text,

        Heal,
        Heal_Critical_Text,

        Miss,

        Stress,

        // Dot
        Bleed,
        Blight,
        Burn,
        Regen,
        Horror,
        Horror_Back_Text,

        // Token
        Daze,
        Stun,
        Taunt,
        Vulnerable,
        Weak,
        Blind,
        Immobilize,


        Combo,
        Combo_Use,
        Guard,
        Block,
        Block_Plus,
        Dodge,
        Dodge_Plus,
        Strength,
        Stealth,
        Riposte,
        CriticalToken,
        Speed,


        Death_Armor_Break,
        Death_Door,
        Death,

        DenialOfArms,
        DenialOfReach,
        DenialOfFortitude,
        DenialOfReason,

        Cure, 

    }
    // 메세지 정렬 방식
    private enum ETextAlignType
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
    [SerializeField] private Sprite _healSprite;
    [SerializeField] private Sprite _deathDoorSprite;
    [SerializeField] private Sprite _deathArmorBreakSprite;

    [SerializeField] private Sprite _bleedSprite;
    [SerializeField] private Sprite _blightSprite;
    [SerializeField] private Sprite _burnSprite;
    [SerializeField] private Sprite _regenSprite;
    [SerializeField] private Sprite _horrorSprite;

    [SerializeField] private Sprite _dazeSprite;
    [SerializeField] private Sprite _stunSprite;
    [SerializeField] private Sprite _tauntSprite;
    [SerializeField] private Sprite _vulnerableSprite;
    [SerializeField] private Sprite _weakSprite;
    [SerializeField] private Sprite _blindSprite;
    [SerializeField] private Sprite _immobilizeSprite;

    [SerializeField] private Sprite _comboSprite;
    [SerializeField] private Sprite _guardSprite;
    [SerializeField] private Sprite _blockSprite;
    [SerializeField] private Sprite _blockPlusSprite;
    [SerializeField] private Sprite _dodgeSprite;
    [SerializeField] private Sprite _dodgePlusSprite;
    [SerializeField] private Sprite _strengthSprite;
    [SerializeField] private Sprite _stealthSprite;
    [SerializeField] private Sprite _riposteSprite;
    [SerializeField] private Sprite _critSprite;
    [SerializeField] private Sprite _speedSprite;

    [SerializeField] private Sprite _denialOfArmsSprite;
    [SerializeField] private Sprite _denialOfReachSprite;
    [SerializeField] private Sprite _denialOfFortitudeSprite;
    [SerializeField] private Sprite _denialOfReasonSprite;

    #endregion

    #region Field
    private RectTransform _rectTr;
    private RectTransform _iconRectTr;
    private RectTransform _textRectTr;
    private bool _stopText = false;
    private EMessageType _type;
    private bool _init = false;
    private bool _rendererCameraMode = false;
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
        PlayAnimation();
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

        // 캔버스 랜더러 모드 체크
        if (!_init)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _rendererCameraMode = true;
            }
        }

        SkinnedMeshRenderer mesh = null;

        if (target.GetComponent<CEntity>() is CEnemyCorpse)
        {
            CEnemyCorpse corpse = target.GetComponent<CEntity>() as CEnemyCorpse;
            mesh = corpse.Owner.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        else
        {
            mesh = target.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (mesh == null)
        {
            return;
        }
       


        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 uiLocalPos;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(mesh.bounds.center);

        _text.text = text;
        _type = type;

        // 다중 메세지 겹침 위치조정
        if (offset != default)
        {
            _rectTr.anchoredPosition += offset;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, _rendererCameraMode ? Camera.main : null, out uiLocalPos);
        _rectTr.anchoredPosition = uiLocalPos;

        switch (type)
        {
            case EMessageType.None:
                SetAlignment(ETextAlignType.Default);
                break;
            // 죽음의 일격!
            case EMessageType.Death:
                SetAlignment(ETextAlignType.Default);
                _text.color = new Color32(213, 68, 17, 255);
                _stopText = true;
                // 죽음의 일격! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 100f;
                _rectTr.localScale = Vector3.one * 5f;
                break;
            case EMessageType.Death_Door:
                SetAlignment(ETextAlignType.Dot);
                _text.color = new Color32(154, 20, 23, 255);
                _stopText = true;
                _iconImage.sprite = _deathDoorSprite;
                // 죽음의 문턱! 메세지 위치조정
                _rectTr.anchoredPosition -= new Vector2(_rectTr.sizeDelta.x * 0.5f, 0);
                _rectTr.localScale = Vector3.one * 5f;
                break;
            case EMessageType.Death_Armor_Break:
                SetAlignment(ETextAlignType.Dot);
                _text.color = new Color32(154, 20, 23, 255);
                _stopText = true;
                _iconImage.sprite = _deathArmorBreakSprite;
                // 죽음 저항 파괴 메세지 위치조정
                _rectTr.anchoredPosition -= new Vector2(_rectTr.sizeDelta.x * 0.5f, 0);
                _rectTr.localScale = Vector3.one * 5f;
                break;
            // 데미지, 치명타데미지, 치명타! 메세지
            case EMessageType.Damage:
                SetAlignment(ETextAlignType.Default);
                _speed = 100;
                _text.color = new Color32(218, 20, 37, 255);
                break;
            case EMessageType.Damage_Critical:
                SetAlignment(ETextAlignType.Default);
                _speed = 100;
                _text.color = new Color32(241, 116, 9, 255);
                break;
            case EMessageType.Damage_Critical_Text:
                SetAlignment(ETextAlignType.Default);
                _speed = 100;
                _text.color = new Color32(241, 116, 9, 255);
                // 치명타! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 50f;
                break;
            case EMessageType.Miss:
                SetAlignment(ETextAlignType.Default);
                _speed = 100;
                _text.color = new Color32(52, 52, 52, 255);
                break;
            case EMessageType.Heal:
                SetAlignment(ETextAlignType.Dot, text);
                _speed = 100;
                _iconImage.sprite = _healSprite;
                _text.color = new Color32(151, 255, 183, 255);
                break;
            case EMessageType.Heal_Critical_Text:
                SetAlignment(ETextAlignType.Default, text);
                _speed = 100;
                _text.color = new Color32(151, 255, 183, 255);
                // 치명타! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 80f;
                break;
            // 스트레스 및 회복
            case EMessageType.Stress:
                SetAlignment(ETextAlignType.Default, text);
                _speed = -30;
                _text.color = new Color32(244, 243, 230, 255);
                Vector2 topVec = new Vector2(mesh.bounds.center.x, mesh.bounds.max.y);
                screenPos = Camera.main.WorldToScreenPoint(topVec);
                break;
            // DOT
            case EMessageType.Bleed:
                SetAlignment(ETextAlignType.Dot, text);
                _iconImage.sprite = _bleedSprite;
                _text.color = new Color32(140, 8, 24, 255);
                break;
            case EMessageType.Blight:
                _iconImage.sprite = _blightSprite;
                SetAlignment(ETextAlignType.Dot, text);
                _text.color = new Color32(43, 82, 60, 255);
                break;
            case EMessageType.Burn:
                _iconImage.sprite = _burnSprite;
                SetAlignment(ETextAlignType.Dot, text);
                _text.color = new Color32(159, 64, 30, 255);
                break;
            case EMessageType.Regen:
                SetAlignment(ETextAlignType.Dot, text);
                _iconImage.sprite = _regenSprite;
                _text.color = new Color32(151, 255, 183, 255);
                break;
            //공포 이중 텍스트
            case EMessageType.Horror:
                SetAlignment(ETextAlignType.Dot, text);
                _iconImage.sprite = _horrorSprite;
                _text.color = new Color32(0, 0, 0, 255);
                break;
            case EMessageType.Horror_Back_Text:
                SetAlignment(ETextAlignType.Dot, text);
                _iconImage.sprite = null;
                _iconFrameImage.enabled = false;
                _iconImage.enabled = false;
                _iconInnerImage.enabled = false;
                _text.color = new Color32(255, 255, 255, 255);
                _rectTr.anchoredPosition += Vector2.right * 2f;
                _rectTr.anchoredPosition += Vector2.down * 2f;
                break;
            // 부정토큰
            case EMessageType.Stun:
                _iconImage.sprite = _stunSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Daze:
                _iconImage.sprite = _dazeSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Taunt:
                _iconImage.sprite = _tauntSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Vulnerable:
                _iconImage.sprite = _vulnerableSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Weak:
                _iconImage.sprite = _weakSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Blind:
                _iconImage.sprite = _blindSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Immobilize:
                _iconImage.sprite = _immobilizeSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            // 콤보
            case EMessageType.Combo:
                _iconImage.sprite = _comboSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Combo_Use:
                SetAlignment(ETextAlignType.Default);
                _speed = 100;
                _text.color = new Color32(81, 183, 160, 255);
                // 연계! 메세지 위치조정
                _rectTr.anchoredPosition += Vector2.up * 50f;
                break;
            // 긍정토큰
            case EMessageType.Block:
                _iconImage.sprite = _blockSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Block_Plus:
                _iconImage.sprite = _blockPlusSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Dodge:
                _iconImage.sprite = _dodgeSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Dodge_Plus:
                _iconImage.sprite = _dodgePlusSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Guard:
                _iconImage.sprite = _guardSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Strength:
                _iconImage.sprite = _strengthSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Stealth:
                _iconImage.sprite = _stealthSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Riposte:
                _iconImage.sprite = _riposteSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.CriticalToken:
                _iconImage.sprite = _critSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Speed:
                _iconImage.sprite = _speedSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            // 보스 특수토큰
            case EMessageType.DenialOfArms:
                _iconImage.sprite = _denialOfArmsSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.DenialOfReach:
                _iconImage.sprite = _denialOfReachSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.DenialOfFortitude:
                _iconImage.sprite = _denialOfFortitudeSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.DenialOfReason:
                _iconImage.sprite = _denialOfReachSprite;
                SetAlignment(ETextAlignType.Token, text);
                break;
            case EMessageType.Cure:
                _text.color = new Color32(255, 255, 255, 255);
                SetAlignment(ETextAlignType.Default, text);
                break;
            default:
                break;

        }


        gameObject.SetActive(true);
        Destroy(gameObject, _duration);
    }

    private void SetAlignment(ETextAlignType textType, string text = null)
    {
        switch (textType)
        {
            case ETextAlignType.Default:
                _textRectTr.anchorMin = new Vector2(0.5f, 0.5f);
                _textRectTr.anchorMax = new Vector2(0.5f, 0.5f);
                _textRectTr.pivot = new Vector2(0.5f, 0.5f);
                _textRectTr.anchoredPosition = new Vector2(0, 0);
                _iconImage.enabled = false;
                _iconFrameImage.enabled = false;
                _iconInnerImage.enabled = false;
                break;
            case ETextAlignType.Dot:
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
            case ETextAlignType.Token:
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

    private void Move()
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

    private void PlayAnimation()
    {
        switch (_type)
        {
            case EMessageType.Death_Armor_Break:
            case EMessageType.Death_Door:
            case EMessageType.Death:
                _rectTr.localScale = Vector3.Lerp(_rectTr.localScale, Vector3.one, 1f - Mathf.Exp(-30 * Time.deltaTime));
                break;
            default:
                break;
        }
    }
}
