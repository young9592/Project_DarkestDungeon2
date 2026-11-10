using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 지시등 자리이동 마우스 이벤트 처리
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
            CPrint.Warn("Target과의 연결이 끊겼습니다.");
            return;
        }

        OnClickRankEvent?.Invoke(_target.CurrentRank);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_target == null)
        {
            CPrint.Warn("Target과의 연결이 끊겼습니다.");
            return;
        }

        _isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_target == null)
        {
            CPrint.Warn("Target과의 연결이 끊겼습니다.");
            return;
        }

        _isHover = false;
    }
}
