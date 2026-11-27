
using UnityEngine;
using UnityEngine.UI;

public class CSkillSlot : MonoBehaviour
{
    [SerializeField] private Image _skillImage;

    private CHeroSkillSO _skillData;

    public void SetSkill(CHeroSkillSO skillData)
    {
        _skillData = skillData;

        if (_skillImage.sprite = null) return;

        _skillImage.sprite = _skillData.SkillIcon;
    }
}
