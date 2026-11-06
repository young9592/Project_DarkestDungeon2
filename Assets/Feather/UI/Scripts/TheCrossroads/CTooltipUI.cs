
using UnityEngine;
using UnityEngine.EventSystems;

public class CTooltipUI : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject _tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.SetActive(true);
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        _tooltip.SetActive(false);
    }

}
