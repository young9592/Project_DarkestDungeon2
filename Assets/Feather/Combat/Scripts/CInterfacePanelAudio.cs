using UnityEngine;

[DisallowMultipleComponent]
public class CInterfacePanelAudio : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private RectTransform _slidingPanel;
    [Header("Slide Position")]
    [SerializeField] private float _openX;
    [SerializeField] private float _closedX;
    private bool _initialized;
    private bool _wasOpen;
    private float _lastX;
    private int _direction;

    private void LateUpdate()
    {
        if (_panel == null && _slidingPanel == null) return;
        if (!_initialized)
        {
            _initialized = true;
            _wasOpen = _panel != null && _panel.activeInHierarchy;
            if (_slidingPanel != null) _lastX = _slidingPanel.anchoredPosition.x;
            return;
        }
        CInterfaceAudio audio = CInterfaceAudio.Instance;
        if (_slidingPanel != null)
        {
            float x = _slidingPanel.anchoredPosition.x;
            float change = x - _lastX;
            _lastX = x;
            if (Mathf.Abs(change) < 0.01f) return;
            int direction = change * (_openX - _closedX) > 0.0f ? 1 : -1;
            if (direction == _direction) return;
            _direction = direction;
            if (audio != null) audio.Play(direction > 0 ? "inventory_open" : "inventory_close");
            return;
        }
        bool open = _panel.activeInHierarchy;
        if (open == _wasOpen) return;
        _wasOpen = open;
        if (audio != null) audio.Play(open ? "menu_open" : "menu_close");
    }
}
