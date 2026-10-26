using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public class CTempLeeTestUI : MonoBehaviour
{
    [Header("Test UI")]
    [SerializeField] private bool _showOnStart;
    [SerializeField] private Key _toggleKey = Key.F8;
    [SerializeField] private CanvasGroup[] _uiGroups;
    [SerializeField] private Button _toggleButton;
    [SerializeField] private Text _label;
    private static CTempLeeTestUI _active;
    private bool _isVisible;

    public bool IsVisible
    {
        get { return _isVisible; }
    }

    public static bool CanShow(Component owner)
    {
        return _active == null || owner == null
            || owner.gameObject.scene != _active.gameObject.scene || _active._isVisible;
    }

    private void Awake()
    {
        _isVisible = _showOnStart;
    }

    private void OnEnable()
    {
        _active = this;
        if (_toggleButton != null) _toggleButton.onClick.AddListener(ToggleVisibility);
        ApplyVisibility(_isVisible);
        RefreshLabel();
    }

    private void Update()
    {
        if (_toggleKey != Key.None && Keyboard.current != null && Keyboard.current[_toggleKey].wasPressedThisFrame)
        {
            ToggleVisibility();
        }
    }

    public void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        if (!visible && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        ApplyVisibility(visible);
        RefreshLabel();
    }

    private void ApplyVisibility(bool visible)
    {
        if (_uiGroups == null) return;
        for (int i = 0; i < _uiGroups.Length; i++)
        {
            CanvasGroup group = _uiGroups[i];
            if (group == null) continue;
            group.alpha = visible ? 1.0f : 0.0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }

    private void ToggleVisibility()
    {
        SetVisible(!_isVisible);
    }

    private void RefreshLabel()
    {
        if (_label != null)
        {
            _label.text = (_isVisible ? "Test UI: ON" : "Test UI: OFF") + " [" + _toggleKey + "]";
        }
    }

    private void OnDisable()
    {
        if (_toggleButton != null) _toggleButton.onClick.RemoveListener(ToggleVisibility);
        if (_active != this) return;
        _active = null;
        ApplyVisibility(true);
    }
}
