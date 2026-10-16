using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CHeroBattleInput : MonoBehaviour
{
    [SerializeField] private CPlayer _hero;
    [SerializeField] private Camera _camera;
    [SerializeField] private CHeroSkillBar _skillBar;

    private readonly List<CHeroTargetView> _targets = new List<CHeroTargetView>();
    private readonly List<CEntity> _groupTargets = new List<CEntity>();
    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();
    private PointerEventData _pointerData;
    private EventSystem _eventSystem;

    public CPlayer Hero { get { return _hero; } }
    public IReadOnlyList<CHeroTargetView> Targets { get { return _targets; } }
    public bool CanChooseSkill
    {
        get { return isActiveAndEnabled && _hero != null && _hero.isActiveAndEnabled && _hero.ActionState == EEntityActionState.Selecting; }
    }

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    private void OnEnable()
    {
        if (_hero == null) return;
        _hero.OnActionRequested += HandleRequested;
        _hero.OnActionCompleted += HandleEnded;
        _hero.OnActionInterrupted += HandleEnded;
    }

    private void OnDisable()
    {
        if (_hero != null)
        {
            _hero.OnActionRequested -= HandleRequested;
            _hero.OnActionCompleted -= HandleEnded;
            _hero.OnActionInterrupted -= HandleEnded;
            if (_hero.ActionState == EEntityActionState.Selecting) _hero.CancelSkillSelection();
        }
        ClearHighlights();
    }

    public void SetTargets(IReadOnlyList<CEnemy> enemies, Vector3 rankStep)
    {
        ClearHighlights();
        _targets.Clear();
        if (CanChooseSkill) _hero.CancelSkillSelection();
        if (enemies == null) return;
        for (int i = 0; i < enemies.Count; i++)
        {
            CEnemy enemy = enemies[i];
            if (enemy == null) continue;
            CHeroTargetView view = enemy.GetComponent<CHeroTargetView>();
            if (view == null) view = enemy.gameObject.AddComponent<CHeroTargetView>();
            if (_targets.Contains(view)) continue;
            view.Configure(enemy, _camera, rankStep);
            _targets.Add(view);
        }
        CHeroTargetView self = _hero.GetComponent<CHeroTargetView>();
        if (self == null) self = _hero.gameObject.AddComponent<CHeroTargetView>();
        self.Configure(_hero, _camera, rankStep);
        _targets.Add(self);
    }

    public void AddAllyTargets(IReadOnlyList<CPlayer> allies, Vector3 rankStep)
    {
        if (allies == null) return;
        for (int i = 0; i < allies.Count; i++)
        {
            CPlayer ally = allies[i];
            if (ally == null || ally == _hero) continue;
            CHeroTargetView view = ally.GetComponent<CHeroTargetView>();
            if (view == null) view = ally.gameObject.AddComponent<CHeroTargetView>();
            if (_targets.Contains(view)) continue;
            view.Configure(ally, _camera, rankStep);
            _targets.Add(view);
        }
    }

    public bool SelectSkill(int index)
    {
        if (!CanChooseSkill) return false;
        bool selected = _hero.TrySelectSkill(index);
        RefreshHighlights(null);
        return selected;
    }

    private bool CanTarget(CHeroTargetView view)
    {
        return CanChooseSkill && view != null && view.Entity != null
            && _hero.CanRequestSkillAction(view.Entity, view.Entity.CurrentRank);
    }

    private void Update()
    {
        if (!CanChooseSkill)
        {
            ClearHighlights();
            return;
        }
        Mouse mouse = Mouse.current;
        if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            || (mouse != null && mouse.rightButton.wasPressedThisFrame))
        {
            _hero.CancelSkillSelection();
            ClearHighlights();
            return;
        }
        CHeroTargetView hovered = null;
        if (mouse != null && _camera != null)
        {
            Vector2 position = mouse.position.ReadValue();
            bool overUI = IsPointerOverUI(position);
            if (!overUI)
            {
                Ray ray = _camera.ScreenPointToRay(position);
                float nearest = float.MaxValue;
                for (int i = 0; i < _targets.Count; i++)
                {
                    CHeroTargetView view = _targets[i];
                    if (!CanTarget(view) || !view.TryGetHit(ray, out float distance) || distance >= nearest) continue;
                    hovered = view;
                    nearest = distance;
                }
                if (hovered != null && mouse.leftButton.wasPressedThisFrame)
                {
                    CHeroSkillSO skill = _hero.SelectedSkill as CHeroSkillSO;
                    if (skill != null && skill.Target == EHeroSkillTarget.AllEnemies)
                    {
                        _groupTargets.Clear();
                        for (int i = 0; i < _targets.Count; i++)
                            if (CanTarget(_targets[i]) && !_groupTargets.Contains(_targets[i].Entity)) _groupTargets.Add(_targets[i].Entity);
                        _hero.TryRequestSkillAction(_groupTargets);
                    }
                    else _hero.TryRequestSkillAction(hovered.Entity, hovered.Entity.CurrentRank);
                }
            }
        }
        RefreshHighlights(hovered);
    }

    private void RefreshHighlights(CHeroTargetView hovered)
    {
        for (int i = 0; i < _targets.Count; i++)
            if (_targets[i] != null) _targets[i].SetHighlight(CanTarget(_targets[i]), _targets[i] == hovered);
    }

    private bool IsPointerOverUI(Vector2 position)
    {
        if (_skillBar != null && _skillBar.ContainsScreenPoint(position)) return true;
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

    private void ClearHighlights()
    {
        for (int i = 0; i < _targets.Count; i++)
            if (_targets[i] != null) _targets[i].SetHighlight(false, false);
    }

    private void HandleRequested(CEntity actor, CEntity target, CSkillDataSO skill) { ClearHighlights(); }
    private void HandleEnded(CEntity actor) { ClearHighlights(); }
}
