using UnityEngine;
using UnityEngine.EventSystems;

public class CPreparationSkillPreview : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private CHeroPreparationPanel _panel;
    [SerializeField] private int _index;
    [SerializeField] private bool _equippedSlot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Preview();
    }

    public void OnSelect(BaseEventData eventData)
    {
        Preview();
    }

    private void Preview()
    {
        if (_panel != null)
        {
            _panel.PreviewSkill(_index, _equippedSlot);
        }
    }
}
