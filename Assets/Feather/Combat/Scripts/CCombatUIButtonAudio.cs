using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ECombatUIButtonRole
{
    HoverOnly,
    Skill,
    Confirm,
    Cancel,
    PressConfirm,
    HoldConfirm
}

[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
public class CCombatUIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CCombatUIAudio _audio;
    [SerializeField] private ECombatUIButtonRole _role;
    [SerializeField] private int _skillIndex = -1;
    [SerializeField, Min(0.0f)] private float _holdDelay = 1.0f;

    private Button _button;
    private bool _hovered;
    private bool _listening;
    private bool _pressed;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Configure(CCombatUIAudio audio, ECombatUIButtonRole role, int skillIndex = -1, float holdDelay = 1.0f)
    {
        _audio = audio;
        _role = role;
        _skillIndex = skillIndex;
        _holdDelay = holdDelay;
    }

    private void OnEnable()
    {
        if (_button == null || _listening) return;
        _button.onClick.AddListener(Clicked);
        _listening = true;
    }

    private void Clicked()
    {
        if (_audio == null || _button == null || !_button.IsInteractable()) return;
        switch (_role)
        {
            case ECombatUIButtonRole.Skill: _audio.QueueSkillClick(_skillIndex); break;
            case ECombatUIButtonRole.Confirm: _audio.QueueConfirm(); break;
            case ECombatUIButtonRole.Cancel: _audio.QueueCancel(); break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_audio == null || _pressed || eventData.button != PointerEventData.InputButton.Left
            || (_button != null && !_button.IsInteractable())) return;
        _pressed = true;
        if (_role == ECombatUIButtonRole.PressConfirm) _audio.QueueConfirm();
        if (_role == ECombatUIButtonRole.HoldConfirm) _audio.BeginHold(_holdDelay);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        if (_audio != null && _role == ECombatUIButtonRole.HoldConfirm) _audio.CancelHold();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || _hovered || _audio == null
            || (_button != null && !_button.IsInteractable())) return;
        _hovered = true;
        _audio.UIHovered();
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
        _pressed = false;
    }
}
