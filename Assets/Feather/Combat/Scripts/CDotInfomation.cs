using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CDotInfomation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Inspector
    [Header("Token Infomation Box")]
    [SerializeField] private GameObject _tooltip;

    [Header("Connect Dot")]
    [SerializeField] private EEffectType _connectEffectType;
    [SerializeField] private int _duration = 0;
    [SerializeField] private int _totalDamage = 0;
    #endregion

    #region Property
    public bool IsInitialize => _tooltip != null;
    #endregion

    public void Initialize(GameObject tooltip)
    {
        _tooltip = tooltip;
    }


    public void SetData(int duration, int totalDamage)
    {
        _totalDamage = totalDamage;
        _duration = duration;
    }
    public void ResetDot()
    {
        _duration = 0;
        _totalDamage = 0;
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
        Transform moveTr = gameObject.GetComponent<Transform>();

        tr.position = moveTr.position;


        CDotText dotText = _tooltip.GetComponent<CDotText>();
        dotText.SetDotInfomation(_connectEffectType, _duration, _totalDamage);
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
