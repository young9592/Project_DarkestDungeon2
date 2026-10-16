using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CHeroSkillBar : MonoBehaviour
{
    [SerializeField] private CHeroBattleInput _input;
    [SerializeField] private Button _buttonPrefab;
    [SerializeField] private Transform _buttonRoot;
    [SerializeField] private Text _status;

    private readonly List<Button> _buttons = new List<Button>();
    private readonly List<CSkillDataSO> _skills = new List<CSkillDataSO>();

    public IReadOnlyList<Button> Buttons { get { return _buttons; } }

    public bool ContainsScreenPoint(Vector2 point)
    {
        return isActiveAndEnabled && RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, point);
    }

    private void Update()
    {
        if (_input == null || _input.Hero == null) return;
        var skills = _input.Hero.EquippedSkills;
        bool changed = skills == null ? _skills.Count != 0 : skills.Count != _skills.Count;
        if (!changed && skills != null)
            for (int i = 0; i < skills.Count; i++) if (skills[i] != _skills[i]) changed = true;
        if (changed) Rebuild();
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].interactable = _input.CanChooseSkill && CSkillRankUtility.CanUseSkill(_skills[i], _input.Hero.CurrentRank);
            _buttons[i].image.color = _input.Hero.SelectedSkill == _skills[i] ? new Color(0.8f, 0.55f, 0.15f) : new Color(0.2f, 0.25f, 0.3f);
        }
        if (_status != null)
            _status.text = !_input.CanChooseSkill ? "Waiting for action" : _skills.Count == 0 ? "No equipped skills"
                : _input.Hero.SelectedSkill == null ? "Choose a skill" : "Click a highlighted enemy / Right click or Esc to cancel";
    }

    private void Rebuild()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].gameObject.SetActive(false);
            Destroy(_buttons[i].gameObject);
        }
        _buttons.Clear();
        _skills.Clear();
        if (_buttonPrefab == null || _buttonRoot == null || _input.Hero.EquippedSkills == null) return;
        for (int i = 0; i < _input.Hero.EquippedSkills.Count; i++)
        {
            CSkillDataSO skill = _input.Hero.EquippedSkills[i];
            Button button = Instantiate(_buttonPrefab, _buttonRoot);
            button.gameObject.SetActive(true);
            Text label = button.GetComponentInChildren<Text>();
            if (label != null) label.text = skill != null ? skill.SkillName : "Empty";
            int index = i;
            button.onClick.AddListener(() => _input.SelectSkill(index));
            _skills.Add(skill);
            _buttons.Add(button);
        }
    }
}
