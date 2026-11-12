using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CTokenInfomation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Inspector
    [Header("Connect Token")]
    [SerializeField] private Image _twoHave;
    [SerializeField] private Image _moreHave;
    [SerializeField] private EEffectType _connectEffectType;
    #endregion
    

    public void SetTokenState()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
