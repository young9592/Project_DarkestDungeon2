using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero/Skill Loadout", fileName = "HeroSkillLoadoutSO_")]
public class CHeroSkillLoadoutSO : ScriptableObject
{
    [SerializeField] private CHeroDataSO _heroData;
    [SerializeField] private List<CSkillDataSO> _skills = new List<CSkillDataSO>();

    public IReadOnlyList<CSkillDataSO> Skills
    {
        get { return _skills; }
    }

    public static CHeroSkillLoadoutSO CreateRuntime(CHeroDataSO heroData, IReadOnlyList<CSkillDataSO> skills)
    {
        if (heroData == null || skills == null)
        {
            return null;
        }

        CHeroSkillLoadoutSO loadout = CreateInstance<CHeroSkillLoadoutSO>();
        loadout.hideFlags = HideFlags.DontSave;
        loadout._heroData = heroData;
        for (int i = 0; i < skills.Count; i++)
        {
            loadout._skills.Add(skills[i]);
        }

        if (loadout.TryValidate(heroData, out _))
        {
            return loadout;
        }

        Destroy(loadout);
        return null;
    }

    public bool TryValidate(CHeroDataSO heroData, out string message)
    {
        if (heroData == null || _heroData != heroData)
        {
            message = "장착 목록의 Hero Data가 캐릭터와 다릅니다.";
            return false;
        }
        if (_skills == null)
        {
            message = "장착 스킬 목록이 없습니다.";
            return false;
        }
        if (_skills.Count > 5)
        {
            message = "스킬은 최대 5개까지 장착할 수 있습니다.";
            return false;
        }
        for (int i = 0; i < _skills.Count; i++)
        {
            CSkillDataSO skill = _skills[i];
            bool available = false;
            if (skill != null && heroData.AvailableSkills != null)
            {
                for (int j = 0; j < heroData.AvailableSkills.Count; j++)
                    if (heroData.AvailableSkills[j] == skill) available = true;
            }
            if (!available)
            {
                message = "영웅이 보유하지 않은 스킬이나 빈 항목이 있습니다.";
                return false;
            }
            for (int j = 0; j < i; j++)
            {
                if (_skills[j] != skill) continue;
                message = "같은 스킬이 중복되어 있습니다.";
                return false;
            }
        }
        message = string.Empty;
        return true;
    }

    private void OnValidate()
    {
        if (!TryValidate(_heroData, out string message)) CPrint.Warn(name + ": " + message);
    }
}
