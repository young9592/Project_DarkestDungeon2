using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CHeroSkillBar : MonoBehaviour
{
    [SerializeField] private CHeroBattleInput _input;
    [SerializeField] private Button _buttonPrefab;
    [SerializeField] private Transform _buttonRoot;
    [SerializeField] private Text _status;
    private readonly List<Button> _buttons = new List<Button>();
    private readonly List<Text> _labels = new List<Text>();
    private readonly List<CSkillDataSO> _skills = new List<CSkillDataSO>();
    public IReadOnlyList<Button> Buttons
    {
        get { return _buttons; }
    }

    public bool ContainsScreenPoint(Vector2 point)
    {
        return isActiveAndEnabled && RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, point);
    }

    private void Update()
    {
        if (_input == null || _input.Hero == null)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].interactable = false;
            }
            if (_status != null)
            {
                _status.text = "Choose a Hero first";
                _status.color = Color.white;
            }
            return;
        }

        var skills = _input.Hero.EquippedSkills;
        bool changed = skills == null ? _skills.Count != 0 : skills.Count != _skills.Count;
        if (!changed && skills != null)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i] != _skills[i])
                {
                    changed = true;
                }
            }
        }

        if (changed)
        {
            Rebuild();
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].interactable = _input.GetSkillUnavailableReason(i).Length == 0;
            _buttons[i].image.color = _input.Hero.SelectedSkill == _skills[i] ? new Color(0.8f, 0.55f, 0.15f) : new Color(0.2f, 0.25f, 0.3f);
            Text label = _labels[i];
            if (label != null)
            {
                CSkillDataSO skill = _skills[i];
                int limit = _input.Hero.SkillUses.GetLimit(skill);
                label.text = skill != null ? skill.SkillName : "Empty";
                if (limit > 0) label.text += " [" + _input.Hero.SkillUses.GetRemaining(skill) + "/" + limit + "]";
            }
        }

        if (_status != null)
        {
            RefreshStatus();
        }
    }

    private void RefreshStatus()
    {
        int inspectedIndex = -1;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                RectTransform rect = _buttons[i].transform as RectTransform;
                if (_buttons[i].gameObject.activeInHierarchy
                    && RectTransformUtility.RectangleContainsScreenPoint(rect, mouse.position.ReadValue()))
                {
                    inspectedIndex = i;
                    break;
                }
            }
        }

        if (inspectedIndex < 0)
        {
            inspectedIndex = _skills.IndexOf(_input.Hero.SelectedSkill);
        }

        _status.color = Color.white;
        if (inspectedIndex < 0)
        {
            _status.text = !_input.CanChooseSkill ? _input.GetSkillUnavailableReason(0)
                : _skills.Count == 0 ? "No equipped skills" : "Choose a skill | Hover over a skill to check availability";
            return;
        }

        CSkillDataSO skill = _skills[inspectedIndex];
        string reason = _input.GetSkillUnavailableReason(inspectedIndex);
        if (reason.Length > 0)
        {
            _status.color = new Color(1.0f, 0.7f, 0.35f);
            _status.text = (skill != null ? skill.SkillName + ": " : "") + reason;
        }
        else
        {
            _status.text = skill.SkillName + ": " + TargetText(skill) + " | Right click / Esc: cancel";
        }

        if (skill != null)
        {
            _status.text += " | Use: " + RankText(skill.UsableRank)
                + " | Target: " + RankText(skill.TargetableRank);
            int limit = _input.Hero.SkillUses.GetLimit(skill);
            if (limit > 0) _status.text += " | Uses remaining: " + _input.Hero.SkillUses.GetRemaining(skill) + "/" + limit;
            if (skill.Cooldown > 0) _status.text += " | Cooldown: " + _input.Hero.SkillUses.GetRemainingCooldown(skill) + "/" + skill.Cooldown;
        }
    }

    private string TargetText(CSkillDataSO skill)
    {
        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        if (heroSkill == null)
        {
            return "Enemy";
        }

        if (heroSkill.RequiresCorpse)
        {
            return "Requires an enemy corpse";
        }

        switch (heroSkill.Target)
        {
            case EHeroSkillTarget.Self:
                return "Click yourself";
            case EHeroSkillTarget.Ally:
                return "Other ally";
            case EHeroSkillTarget.AllEnemies:
                return "All eligible enemies";
            case EHeroSkillTarget.AllAllies:
                return "All allies";
            case EHeroSkillTarget.AllyOrSelf:
                return "Ally or yourself";
            default:
                return "Enemy";
        }
    }

    private string RankText(ERank ranks)
    {
        string text = string.Empty;
        for (int i = 0; i < 4; i++)
        {
            if (((int)ranks & (1 << i)) != 0)
            {
                text += (text.Length == 0 ? "" : ",") + (i + 1);
            }
        }

        return text.Length == 0 ? "None" : text;
    }

    private void Rebuild()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].gameObject.SetActive(false);
            Destroy(_buttons[i].gameObject);
        }

        _buttons.Clear();
        _labels.Clear();
        _skills.Clear();
        if (_buttonPrefab == null || _buttonRoot == null || _input.Hero.EquippedSkills == null)
        {
            return;
        }

        for (int i = 0; i < _input.Hero.EquippedSkills.Count; i++)
        {
            CSkillDataSO skill = _input.Hero.EquippedSkills[i];
            Button button = Instantiate(_buttonPrefab, _buttonRoot);
            button.gameObject.SetActive(true);
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = skill != null ? skill.SkillName : "Empty";
            }

            if (skill != null && skill.SkillIcon != null)
            {
                GameObject iconObject = new GameObject("Skill Icon", typeof(RectTransform), typeof(Image));
                iconObject.transform.SetParent(button.transform, false);
                RectTransform iconRect = iconObject.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0, 0.5f);
                iconRect.anchorMax = new Vector2(0, 0.5f);
                iconRect.pivot = new Vector2(0, 0.5f);
                iconRect.anchoredPosition = new Vector2(4, 0);
                iconRect.sizeDelta = new Vector2(40, 40);
                Image icon = iconObject.GetComponent<Image>();
                icon.sprite = skill.SkillIcon;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                if (label != null)
                {
                    label.rectTransform.offsetMin = new Vector2(46, 0);
                }
            }

            int index = i;
            button.onClick.AddListener(() => _input.SelectSkill(index));
            _skills.Add(skill);
            _buttons.Add(button);
            _labels.Add(label);
        }
    }
}
