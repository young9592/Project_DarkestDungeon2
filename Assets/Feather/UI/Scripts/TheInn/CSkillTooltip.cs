
using UnityEngine;
using UnityEngine.EventSystems;

public class CSkillTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    [SerializeField] private GameObject _tooltipUI;
    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltipUI.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltipUI.SetActive(false);
    }
}