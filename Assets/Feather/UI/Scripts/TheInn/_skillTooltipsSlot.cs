
using TMPro;
using UnityEngine;

public class _skillTooltipsSlot : MonoBehaviour
{
    [Header("Skill Name")]
    [SerializeField] private TMP_Text _skillNameText;

    [Header("Skill Description")]
    [SerializeField] private TMP_Text _baseSkillText;
    [SerializeField] private TMP_Text _masterySkillText;

    public void SetTooltip(string skillName, string baseSkill,string masterySkill)
    {
        _skillNameText.text = skillName;
        _baseSkillText.text = baseSkill;
        _masterySkillText.text = masterySkill;
    }
}
