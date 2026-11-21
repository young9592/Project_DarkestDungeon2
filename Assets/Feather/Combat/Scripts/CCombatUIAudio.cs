using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum ECombatUICue
{
    SkillSelect,
    TargetSelect,
    Cancel,
    Confirm,
    Hover,
    EnemyHover,
    Pass,
    Invalid
}

[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
public class CCombatUIAudio : MonoBehaviour
{
    [Header("Battle")]
    [SerializeField] private CCombatManager _combatManager;
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private CHeroBattleInput _battleInput;
    [SerializeField] private Camera _camera;
    [Header("Audio")]
    [SerializeField] private CCombatUIAudioSO _sounds;
    [SerializeField] private AudioSource _source;

    private readonly List<CPlayer> _heroes = new List<CPlayer>();
    private readonly List<GameObject> _portraits = new List<GameObject>();
    private readonly Dictionary<ECombatUICue, int> _playedFrames = new Dictionary<ECombatUICue, int>();
    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();
    private CInputDispatcher _dispatcher;
    private EventSystem _eventSystem;
    private PointerEventData _pointerData;
    private CPlayer _hero;
    private CSkillDataSO _selectedSkill;
    private CEntity _hoveredTarget;
    private int _pendingSkill = -1;
    private int _targetFrame = -1;
    private bool _pendingConfirm;
    private bool _pendingCancel;
    private CPlayer _holdHero;
    private float _holdReadyAt;

    public event Action<ECombatUICue, AudioClip> OnSoundPlayed;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
        if (_combatManager == null || _combatUI == null || _battleInput == null || _sounds == null || _source == null)
            enabled = false;
    }

    private void OnEnable()
    {
        _hero = _combatManager != null ? _combatManager.MyTurn as CPlayer : null;
        _selectedSkill = _hero != null ? _hero.SelectedSkill : null;
        BindDispatcher();
        if (_combatManager != null && _combatUI != null) Bind();
    }

    private void Start() { BindDispatcher(); }

    private void BindDispatcher()
    {
        if (_dispatcher != null) return;
        _dispatcher = CInputDispatcher.Instance;
        if (_dispatcher != null) _dispatcher.OnSkill += KeyboardSkill;
    }

    private void KeyboardSkill(int index)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || index < 0 || index >= 5
            || !keyboard[(Key)((int)Key.Digit1 + index)].wasPressedThisFrame) return;
        QueueSkillClick(index);
    }

    public void QueueSkillClick(int index)
    {
        if (!isActiveAndEnabled) return;
        _pendingSkill = index;
        CPlayer hero = _combatManager.MyTurn as CPlayer;
        IReadOnlyList<CSkillDataSO> skills = hero != null ? hero.EquippedSkills : null;
        if (hero == null || hero.ActionState != EEntityActionState.Selecting
            || skills == null || index < 0 || index >= skills.Count
            || hero.SelectedSkill == null || hero.SelectedSkill != skills[index]) return;
        _hero = hero;
        SelectSound(hero.SelectedSkill);
    }
    public void QueueConfirm() { if (isActiveAndEnabled) _pendingConfirm = true; }
    public void QueueCancel() { if (isActiveAndEnabled) _pendingCancel = true; }
    public void UIHovered() { if (isActiveAndEnabled) Play(ECombatUICue.Hover, _sounds.Hover); }

    public void BeginHold(float delay)
    {
        if (!isActiveAndEnabled) return;
        _holdHero = _combatManager.MyTurn as CPlayer;
        _holdReadyAt = Time.time + Mathf.Max(0.0f, delay);
    }

    public void CancelHold() { _holdHero = null; }

    private void ActionInterrupted(CEntity actor)
    {
        if (_holdHero != actor || Time.time + 0.001f < _holdReadyAt) return;
        _holdHero = null;
        Play(ECombatUICue.Pass, _sounds.Pass);
    }

    private void Bind()
    {
        IReadOnlyList<CPlayer> heroes = _combatManager.HeroParty;
        for (int i = 0; heroes != null && i < heroes.Count; i++)
        {
            CPlayer hero = heroes[i];
            if (hero == null || _heroes.Contains(hero)) continue;
            _heroes.Add(hero);
            hero.OnActionRequested += TargetRequested;
            hero.OnActionInterrupted += ActionInterrupted;
        }
        IReadOnlyList<GameObject> portraits = _combatUI.TurnPortraitList;
        for (int i = 0; portraits != null && i < portraits.Count; i++)
        {
            GameObject portrait = portraits[i];
            if (portrait == null || _portraits.Contains(portrait)) continue;
            _portraits.Add(portrait);
            CCombatUIButtonAudio hover = portrait.GetComponent<CCombatUIButtonAudio>();
            if (hover == null) hover = portrait.AddComponent<CCombatUIButtonAudio>();
            hover.Configure(this, ECombatUIButtonRole.HoverOnly);
        }
    }

    private void LateUpdate()
    {
        Bind();
        CPlayer hero = _combatManager.MyTurn as CPlayer;
        if (_hero != hero)
        {
            _hero = hero;
            _selectedSkill = null;
            _hoveredTarget = null;
        }

        CSkillDataSO selected = _hero != null ? _hero.SelectedSkill : null;
        if (_hero != null && _hero.ActionState == EEntityActionState.Selecting)
        {
            if (selected != null && (selected != _selectedSkill || IsClickedSkill(selected))) SelectSound(selected);
            bool cancelled = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);
            if ((_selectedSkill != null && selected == null && cancelled) || _pendingCancel)
                Play(ECombatUICue.Cancel, _sounds.Cancel);
            IReadOnlyList<CSkillDataSO> equipped = _hero.EquippedSkills;
            if (_pendingSkill >= 0 && equipped != null && _pendingSkill < equipped.Count
                && selected != equipped[_pendingSkill]) Play(ECombatUICue.Invalid, _sounds.Invalid);
            _selectedSkill = selected;
        }
        else
        {
            _selectedSkill = null;
        }

        if (_holdHero != null && _combatUI.Pass && Time.time + 0.001f >= _holdReadyAt)
        {
            _holdHero = null;
            Play(ECombatUICue.Pass, _sounds.Pass);
        }
        if (_pendingConfirm && _targetFrame != Time.frameCount) Play(ECombatUICue.Confirm, _sounds.Confirm);
        if (_pendingCancel) Play(ECombatUICue.Cancel, _sounds.Cancel);
        _pendingSkill = -1;
        _pendingConfirm = false;
        _pendingCancel = false;
        UpdateTargetHover();
    }

    private bool IsClickedSkill(CSkillDataSO selected)
    {
        IReadOnlyList<CSkillDataSO> skills = _hero.EquippedSkills;
        return _pendingSkill >= 0 && skills != null && _pendingSkill < skills.Count && skills[_pendingSkill] == selected;
    }

    private void SelectSound(CSkillDataSO skill)
    {
        _selectedSkill = skill;
        Play(ECombatUICue.SkillSelect, _sounds.Select);
    }

    private void TargetRequested(CEntity actor, CEntity target, CSkillDataSO skill)
    {
        if (!isActiveAndEnabled || actor != _combatManager.MyTurn || !(actor is CPlayer)) return;
        if (_selectedSkill != skill) SelectSound(skill);
        _targetFrame = Time.frameCount;
        Play(ECombatUICue.TargetSelect, _sounds.Confirm);
        _selectedSkill = null;
        _hoveredTarget = null;
    }

    private void UpdateTargetHover()
    {
        CEntity hovered = null;
        Mouse mouse = Mouse.current;
        if (_hero != null && _battleInput.CanChooseSkill && mouse != null && _camera != null
            && !IsPointerOverUI(mouse.position.ReadValue()))
        {
            Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
            float nearest = float.MaxValue;
            IReadOnlyList<CHeroTargetView> views = _battleInput.Targets;
            for (int i = 0; views != null && i < views.Count; i++)
            {
                CHeroTargetView view = views[i];
                if (view == null || view.Entity == null || view.Entity.IsDead
                    || !view.TryGetHit(ray, out float distance) || distance >= nearest) continue;
                hovered = view.Entity;
                nearest = distance;
            }
        }
        if (hovered != null && hovered != _hoveredTarget)
            Play(hovered is CEnemy ? ECombatUICue.EnemyHover : ECombatUICue.Hover,
                hovered is CEnemy ? _sounds.EnemyHover : _sounds.Hover);
        _hoveredTarget = hovered;
    }

    private bool IsPointerOverUI(Vector2 position)
    {
        EventSystem current = EventSystem.current;
        if (current == null) return false;
        if (_eventSystem != current)
        {
            _eventSystem = current;
            _pointerData = new PointerEventData(current);
        }
        _pointerData.position = position;
        _uiHits.Clear();
        current.RaycastAll(_pointerData, _uiHits);
        for (int i = 0; i < _uiHits.Count; i++)
            if (_uiHits[i].module is GraphicRaycaster) return true;
        return false;
    }

    private void Play(ECombatUICue cue, AudioClip clip)
    {
        if (clip == null || !_source.isActiveAndEnabled
            || (_playedFrames.TryGetValue(cue, out int frame) && frame == Time.frameCount)) return;
        _playedFrames[cue] = Time.frameCount;
        _source.PlayOneShot(clip, _sounds.Volume);
        OnSoundPlayed?.Invoke(cue, clip);
    }

    private void OnDisable()
    {
        for (int i = 0; i < _heroes.Count; i++)
        {
            if (_heroes[i] == null) continue;
            _heroes[i].OnActionRequested -= TargetRequested;
            _heroes[i].OnActionInterrupted -= ActionInterrupted;
        }
        _heroes.Clear();
        _holdHero = null;
        if (_dispatcher != null) _dispatcher.OnSkill -= KeyboardSkill;
        _dispatcher = null;
        _selectedSkill = null;
        _hoveredTarget = null;
        _pendingSkill = -1;
        _pendingConfirm = false;
        _pendingCancel = false;
        if (_source != null) _source.Stop();
    }
}
