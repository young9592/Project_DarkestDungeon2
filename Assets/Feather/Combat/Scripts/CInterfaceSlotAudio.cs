using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CInterfaceSlotAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    private CInterfaceAudio _audio;
    private CSlot _inventorySlot;
    private CStoreSlot _storeSlot;
    private bool _hovered;
    private bool _hadItemOnPress;

    private void Awake()
    {
        _inventorySlot = GetComponent<CSlot>();
        _storeSlot = GetComponent<CStoreSlot>();
    }

    public void Configure(CInterfaceAudio audio)
    {
        _audio = audio;
        if (_inventorySlot == null) _inventorySlot = GetComponent<CSlot>();
        if (_storeSlot == null) _storeSlot = GetComponent<CStoreSlot>();
    }

    private bool HasItem()
    {
        if (_inventorySlot != null) return _inventorySlot.item != null;
        return _storeSlot != null && _storeSlot.storeItem != null && _storeSlot.storeItem._item != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hovered || !isActiveAndEnabled || !HasItem() || _audio == null) return;
        _hovered = true;
        _audio.Play("hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _hadItemOnPress = eventData.button == PointerEventData.InputButton.Left && HasItem();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool valid = _hadItemOnPress || HasItem();
        _hadItemOnPress = false;
        if (eventData.button == PointerEventData.InputButton.Left && valid && _audio != null)
            _audio.Play("select");
    }

    private void OnDisable()
    {
        _hovered = false;
        _hadItemOnPress = false;
    }
}
