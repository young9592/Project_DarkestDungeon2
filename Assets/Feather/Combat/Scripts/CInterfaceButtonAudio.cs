using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CInterfaceButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CInterfaceAudio _audio;
    private Button _button;
    private bool _hovered;
    private bool _listening;
    private string _clickKey = "select";

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Configure(CInterfaceAudio audio)
    {
        _audio = audio;
        if (_button == null) _button = GetComponent<Button>();
        if (_button == null) return;
        for (int i = 0; i < _button.onClick.GetPersistentEventCount(); i++)
        {
            string method = _button.onClick.GetPersistentMethodName(i).ToLowerInvariant();
            if (method.Contains("close") || method.Contains("cancel") || method.Contains("back")
                || method.Contains("exit")) _clickKey = "cancel";
        }
    }

    private void OnEnable()
    {
        if (_button == null || _listening) return;
        _button.onClick.AddListener(Clicked);
        _listening = true;
    }

    private void Clicked()
    {
        if (_audio != null && _button != null && _button.IsInteractable()) _audio.Play(_clickKey);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hovered || !isActiveAndEnabled || _audio == null || _button == null || !_button.IsInteractable()) return;
        _hovered = true;
        _audio.Play("hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
    }

    private void OnDisable()
    {
        if (_button != null && _listening) _button.onClick.RemoveListener(Clicked);
        _listening = false;
        _hovered = false;
    }
}
