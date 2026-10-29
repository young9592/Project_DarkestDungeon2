using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CHeroSkillDetailView : MonoBehaviour
{
    [Header("Skill Detail")]
    [SerializeField] private Image _icon;
    [SerializeField] private Text _name;
    [SerializeField] private Text _target;
    [SerializeField] private Text _effects;
    [SerializeField] private Text _availability;
    [Header("Ranks")]
    [SerializeField] private Image[] _useRanks;
    [SerializeField] private Image[] _targetRanks;
    private CSkillDataSO _displayedSkill;

    public CSkillDataSO DisplayedSkill
    {
        get { return _displayedSkill; }
    }

    public void Show(CSkillDataSO skill, ERank currentRank, bool equipped)
    {
        _displayedSkill = skill;
        if (skill == null)
        {
            if (_name != null)
            {
                _name.text = "Choose a skill";
            }
            if (_icon != null)
            {
                _icon.enabled = false;
            }
            if (_effects != null)
            {
                _effects.text = "Hover over a skill to inspect it.";
            }
            if (_target != null)
            {
                _target.text = "";
            }
            if (_availability != null)
            {
                _availability.text = "";
            }
            ShowRanks(_useRanks, ERank.None);
            ShowRanks(_targetRanks, ERank.None);
            return;
        }

        if (_name != null)
        {
            _name.text = skill.SkillName;
        }
        if (_icon != null)
        {
            _icon.sprite = skill.SkillIcon;
            _icon.enabled = skill.SkillIcon != null;
        }

        CHeroSkillSO heroSkill = skill as CHeroSkillSO;
        if (_target != null)
        {
            _target.text = TargetText(heroSkill);
        }
        if (_effects != null)
        {
            _effects.text = EffectText(skill, heroSkill);
        }
        if (_availability != null)
        {
            _availability.text = equipped ? "Equipped" : "Not equipped";
            _availability.text += CSkillRankUtility.CanUseSkill(skill, currentRank)
                ? "\nUsable from this position" : "\nChange position to use this skill";
        }

        ShowRanks(_useRanks, skill.UsableRank);
        ShowRanks(_targetRanks, skill.TargetableRank);
    }

    private void ShowRanks(Image[] cells, ERank ranks)
    {
        if (cells == null) return;
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == null) continue;
            cells[i].color = ((int)ranks & (1 << i)) != 0
                ? new Color(0.65f, 0.45f, 0.17f) : new Color(0.12f, 0.15f, 0.18f);
        }
    }

    private string TargetText(CHeroSkillSO skill)
    {
        if (skill == null) return "One enemy";
        if (skill.RequiresCorpse) return "Enemy corpse";
        switch (skill.Target)
        {
            case EHeroSkillTarget.Ally: return "One other ally";
            case EHeroSkillTarget.Self: return "Self";
            case EHeroSkillTarget.AllyOrSelf: return "One ally or self";
            case EHeroSkillTarget.AllEnemies: return "All enemies in target ranks";
            case EHeroSkillTarget.AllAllies: return "All allies in target ranks";
            default: return "One enemy";
        }
    }

    private string EffectValueText(CEffectSO effect)
    {
        if (effect.MaxDamage > effect.Damage)
        {
            return effect.Damage + "-" + effect.MaxDamage;
        }

        return effect.Damage.ToString();
    }

    private string EffectText(CSkillDataSO skill, CHeroSkillSO heroSkill)
    {
        StringBuilder text = new StringBuilder();
        bool hasDamage = false;
        if (skill.EffectList != null)
        {
            for (int i = 0; i < skill.EffectList.Count; i++)
            {
                CEffectSO effect = skill.EffectList[i];
                if (effect == null || effect.Strategy == null) continue;
                if (effect.Strategy is CHeroCorpseSkillStrategy corpseSkill)
                {
                    text.Append(heroSkill != null && heroSkill.RequiresCorpse ? "Clear one enemy corpse\n" : "Clear all enemy corpses\n");
                    if (corpseSkill.HealPercent > 0) text.Append("On hit: restore ").Append(corpseSkill.HealPercent).Append("% maximum HP\n");
                    if (corpseSkill.StealthTokens > 0) text.Append("On hit: Stealth x").Append(corpseSkill.StealthTokens).Append('\n');
                    if (corpseSkill.StressRecovery > 0) text.Append("On hit: recover ").Append(corpseSkill.StressRecovery).Append(" stress\n");
                    if (corpseSkill.ResetsCooldowns) text.Append("Clearing a corpse resets this Hero's skill cooldowns\n");
                    text.Append("Corpse clear also occurs on a miss\n");
                    continue;
                }
                if (effect.EffectType == EEffectType.Damage)
                {
                    text.Append("Base damage: ").Append(EffectValueText(effect)).Append(" per target\n");
                    hasDamage = true;
                }
                else if (effect.EffectType == EEffectType.Heal)
                {
                    text.Append("Restore ").Append(EffectValueText(effect)).Append(" HP\nUp to maximum HP\n");
                }
                else
                {
                    text.Append(effect.Name).Append('\n');
                }
            }
        }

        if (text.Length == 0)
        {
            text.Append("No effect connected yet.\nAnimation preview only.\n");
        }
        if (heroSkill != null && heroSkill.UsesPerBattle > 0) text.Append("Uses per battle: ").Append(heroSkill.UsesPerBattle).Append("\n");
        if (skill.Cooldown > 0) text.Append("Cooldown: ").Append(skill.Cooldown).Append(" Hero turn(s)\n");
        if (hasDamage)
        {
            text.Append("Combat effects can change damage.\n");
        }
        if (heroSkill != null && heroSkill.RequiresCorpse)
        {
            text.Append("Requires an enemy corpse.");
        }

        return text.ToString().TrimEnd();
    }
}
