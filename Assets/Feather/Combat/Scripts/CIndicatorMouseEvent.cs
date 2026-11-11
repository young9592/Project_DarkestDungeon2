using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Áö½Ãµî ÀÚ¸®ÀÌµ¿ ¸¶¿ì½º ÀÌº¥Æ® Ã³¸®
public class CIndicatorMouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    #region Field
    private CEntity _target;
    private bool _isHover = false;
    #endregion

    #region Property
    public bool IsHover => _isHover; 
    #endregion

    #region Event
    public static event Action<ERank> OnClickRankEvent;
    #endregion

    public void Initialize(CEntity target)
    {
        _target = target;

        if(CPrint.NullUnityObjectCheck(gameObject.name, _target))
        {
            enabled = false;
            return;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_target == null)
        {
            CPrint.Warn($"{gameObject.name}Å¸°ÙÀÌ¶û ¿¬°áÀÌ ²÷°å½À´Ï´Ù.");
            enabled = false;
            return;
        }

        OnClickRankEvent?.Invoke(_target.CurrentRank);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_target == null)
        {
            CPrint.Warn($"{gameObject.name}Å¸°ÙÀÌ¶û ¿¬°áÀÌ ²÷°å½À´Ï´Ù.");
            enabled = false;
            return;
        }

        _isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_target == null)
        {
            CPrint.Warn($"{gameObject.name}Å¸°ÙÀÌ¶û ¿¬°áÀÌ ²÷°å½À´Ï´Ù.");
            enabled = false;
            return;
        }

        _isHover = false;
    }
}
