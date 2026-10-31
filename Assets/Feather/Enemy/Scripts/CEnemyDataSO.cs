using System.Collections.Generic;
using UnityEngine;

public enum EEnemyTargetSelection
{
    Random,
    Frontmost,
    Rearmost
}

[CreateAssetMenu(menuName = "Data/Entity/Enemy", fileName = "EnemySO_")]
public class CEnemyDataSO : CEntityDataSO
{
    #region Inspector
    [Header("Formation")]
    [Range(1, 4)]
    [SerializeField] private int _formationSize = 1;

    [Header("DeathDoorCount")]
    [SerializeField] private int _deathDoorCount = 0;

    [Header("Corpse")]
    [SerializeField] private bool _leavesCorpse = true;
    [SerializeField] private Sprite _corpseSprite;
    [Tooltip("원작의 적별 시체 체력을 확인하기 전까지 사용하는 테스트 값입니다.")]
    [SerializeField, Min(1)] private int _corpseHealth = 5;
    [SerializeField, Min(0.001f)] private float _corpseWidth = 0.014f;

    [Header("Skill Selection")]
    [Tooltip("공통 스킬 목록에 있는 스킬의 선택 규칙입니다. 설정하지 않은 스킬은 체력 제한 없이 비중 1로 선택합니다.")]
    [SerializeField] private List<CEnemySkillRule> _skillRules = new List<CEnemySkillRule>();
    [SerializeField] private EEnemyTargetSelection _targetSelection;
    #endregion

    #region Property
    public int DeathDoorCount => _deathDoorCount;
    public bool LeavesCorpse { get { return _leavesCorpse; } }
    public Sprite CorpseSprite { get { return _corpseSprite; } }
    public int CorpseHealth { get { return Mathf.Max(1, _corpseHealth); } }
    public float CorpseWidth { get { return Mathf.Max(0.001f, _corpseWidth); } }
    public EEnemyTargetSelection TargetSelection
    {
        get { return _targetSelection; }
    }
    public int FormationSize
    {
        get { return _formationSize; }
    }
    #endregion

    public CEnemySkillRule GetSkillRule(CSkillDataSO skill)
    {
        if (skill == null || _skillRules == null) return null;

        // 같은 스킬을 중복 설정하면 목록에서 먼저 나온 규칙을 사용합니다.
        for (int i = 0; i < _skillRules.Count; i++)
        {
            CEnemySkillRule rule = _skillRules[i];
            if (rule != null && rule.Skill == skill) return rule;
        }

        return null;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        _formationSize = Mathf.Clamp(_formationSize, 1, 4);

        _deathDoorCount = Mathf.Max(0, _deathDoorCount);
        if (_skillRules == null) return;

        List<CSkillDataSO> configuredSkills = new List<CSkillDataSO>();
        for (int i = 0; i < _skillRules.Count; i++)
        {
            CEnemySkillRule rule = _skillRules[i];
            if (rule == null) continue;

            rule.Validate();
            if (rule.Skill == null) continue;
            if (configuredSkills.Contains(rule.Skill))
            {
                CPrint.Warn(name + ": 같은 스킬의 선택 규칙이 중복되어 있습니다. 첫 번째 규칙을 사용합니다.");
                continue;
            }

            configuredSkills.Add(rule.Skill);
        }
    }
}
