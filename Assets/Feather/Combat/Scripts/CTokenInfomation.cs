using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CTokenInfomation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Inspector
    [Header("Token Infomation Box")]
    [SerializeField] private GameObject _tooltip;

    [Header("Connect Token")]
    [SerializeField] private GameObject _twoHave;
    [SerializeField] private GameObject _moreHave;
    [SerializeField] private EEffectType _connectEffectType;
    [SerializeField] private int _duration = 0;
    [SerializeField] private int _stack = 0;
    #endregion

    #region Property
    public bool IsInitialize => _tooltip != null;
    #endregion

    public void Initialize(GameObject tooltip)
    {
        _tooltip = tooltip;
    }

    public void SetTokenState(int duration, int stack)
    {
        _duration = duration;
        _stack = stack;

        // 토큰 보유 3개이상
        if (stack > 2)
        {
            _twoHave.SetActive(true);
            _moreHave.SetActive(true);
        }
        else if (stack > 1)
        {
            _twoHave.SetActive(true);
            _moreHave.SetActive(false);
        }
        else if (stack == 1)
        {
            _twoHave.SetActive(false);
            _moreHave.SetActive(false);
        }
        else
        {
            _twoHave.SetActive(false);
            _moreHave.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    public void ResetToken()
    {
        _twoHave.SetActive(false);
        _moreHave.SetActive(false);
        _duration = 0;
        _stack = 0;

        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip == null)
        {
            return;
        }

        if (!_tooltip.activeSelf)
        {
            _tooltip.SetActive(true);
        }

        Transform tr = _tooltip.GetComponent<Transform>();
        RectTransform rectTr = _tooltip.GetComponent<RectTransform>();
        Transform moveTr = gameObject.GetComponent<Transform>();

        tr.position = moveTr.position;
        rectTr.anchoredPosition += Vector2.up * 50f;
       
        CTokenText tokenText = _tooltip.GetComponent<CTokenText>();
        tokenText.SetTokenInfomation(_connectEffectType, _duration, _stack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip == null)
        {
            return;
        }

        if (_tooltip.activeSelf)
        {
            _tooltip.SetActive(false);
        }
    }
}
