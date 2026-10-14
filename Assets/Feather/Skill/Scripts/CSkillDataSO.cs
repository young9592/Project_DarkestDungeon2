using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill/SkillData", fileName = "SkillSO_")]
public class CSkillDataSO : ScriptableObject
{
    #region Inspector
    [Header("Skill Info")]
    [SerializeField] private string _skillName = "기본 공격";
    [SerializeField] private Sprite _skillIcon;

    [Header("Targeting Rules")]
    // 스킬 사용자가 위치해야 하는 열
    [SerializeField] private int _useableRank;
    // 스킬이 타겟을 잡을 수 있는 적의 열
    [SerializeField] private int _targetableRank;

    [Header("Settings")]
    [SerializeField] private int _cooldown = 0;

    [Header("Effects")]
    // 스킬 발동시 순차적으로 적용될 효과들
    [SerializeField] private List<CEffectSO> _effectList = new List<CEffectSO>();
    #endregion

    #region Property
    public string SkillName => _skillName;
    public Sprite SkillIcon => _skillIcon;
    public int UsableRank => _useableRank;
    public int TargetableRank => _targetableRank;
    public int Cooldown => _cooldown;
    public IReadOnlyList<CEffectSO> EffectList => _effectList; 
    #endregion
}
