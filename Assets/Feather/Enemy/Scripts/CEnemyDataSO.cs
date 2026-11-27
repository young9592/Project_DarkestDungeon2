using System.Collections.Generic;
using UnityEngine;

public enum EEnemyTargetSelection
{
    Random,
    Frontmost,
    Rearmost
}

public enum EEnemyCategory
{
    None = 0,
    Gaunt,      // Lost Soul, Ghoul
    Beast,      // Rabid Dog, Spitter
    Fanatic,    // Firemouth
    Cosmic,     // Shackles of Denial
    Cultist,    // 사교도
    Pillager,   // 약탈자
    Cadaver,    // 시체
    Swine
}

[CreateAssetMenu(menuName = "Data/Entity/Enemy", fileName = "EnemySO_")]
public class CEnemyDataSO : CEntityDataSO
{
    #region Inspector
    [Header("Enemy Category")]
    [SerializeField] private EEnemyCategory _enemyCategory = EEnemyCategory.None;

    [Tooltip("UI에 직접 표시할 한글 종족명이 필요할 시 입력 (비워두면 Category 기준 자동 반환됨")]
    [SerializeField] private string _enemyTypeDisplayName;

    [Header("Formation")]
    [Range(1, 4)]
    [SerializeField] private int _formationSize = 1;

    [Header("DeathDoorCount")]
    [SerializeField] private int _deathDoorCount = 0;

    [Header("Corpse")]
    [SerializeField] private bool _leavesCorpse = true;
    [SerializeField] private Sprite _corpseSprite;
    [SerializeField] private CEnemyCorpseDataSO _corpseData;
    [Tooltip("시체 전용 데이터가 없는 적의 기본 체력입니다.")]
    [SerializeField, Min(1)] private int _corpseHealth = 5;
    [SerializeField, Min(0.001f)] private float _corpseWidth = 0.014f;

    [Header("Skill Selection")]
    [Tooltip("공통 스킬 목록에 있는 스킬의 선택 규칙입니다. 설정하지 않은 스킬은 체력 제한 없이 비중 1로 선택합니다.")]
    [SerializeField] private List<CEnemySkillRule> _skillRules = new List<CEnemySkillRule>();
    [SerializeField] private EEnemyTargetSelection _targetSelection;
    [SerializeField] private CSkillDataSO _comboPrioritySkill;

    [Header("Starting Token")]
    [SerializeField] private EEffectType _startingToken;
    [SerializeField, Min(0)] private int _startingTokenCount;
    [SerializeField, Range(0, 100)] private int _extraStartingTokenChance;
    #endregion

    #region Property
    public EEnemyCategory EnemyCategory
    {
        get { return _enemyCategory; }
    }
    public string EnemyTypeDisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(_enemyTypeDisplayName))
            {
                return _enemyTypeDisplayName;
            }

            switch (_enemyCategory)
            {
                case EEnemyCategory.Gaunt:      return "실성자";
                case EEnemyCategory.Beast:      return "짐승";
                case EEnemyCategory.Fanatic:    return "광신도";
                case EEnemyCategory.Cultist:    return "사교도";
                case EEnemyCategory.Cosmic:     return "우주적 존재";
                case EEnemyCategory.Pillager:   return "약탈자";
                case EEnemyCategory.Cadaver:    return "시체";
                case EEnemyCategory.Swine:      return "스와인";
                default:                        return "적";
            }
        }
    }
    public int DeathDoorCount => _deathDoorCount;
    public EEffectType StartingToken
    {
        get { return _startingToken; }
    }
    public int StartingTokenCount
    {
        get { return Mathf.Max(0, _startingTokenCount); }
    }
    public int ExtraStartingTokenChance
    {
        get { return Mathf.Clamp(_extraStartingTokenChance, 0, 100); }
    }
    public bool LeavesCorpse
    {
        get { return _leavesCorpse; }
    }
    public Sprite CorpseSprite
    {
        get { return _corpseSprite; }
    }
    public CEnemyCorpseDataSO CorpseData
    {
        get { return _corpseData; }
    }
    public int CorpseHealth
    {
        get { return _corpseData != null ? _corpseData.MaxHealth : Mathf.Max(1, _corpseHealth); }
    }
    public float CorpseWidth
    {
        get { return Mathf.Max(0.001f, _corpseWidth); }
    }
    public EEnemyTargetSelection TargetSelection
    {
        get { return _targetSelection; }
    }
    public CSkillDataSO ComboPrioritySkill
    {
        get { return _comboPrioritySkill; }
    }
    public int FormationSize
    {
        get { return _formationSize; }
    }
    #endregion

    public CEnemySkillRule GetSkillRule(CSkillDataSO skill)
    {
        if (skill == null || _skillRules == null) return null;

        // 같은 스킬을 중복 설정하면 목록에서 먼저 나온 규칙을 사용함
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
        _startingTokenCount = Mathf.Max(0, _startingTokenCount);
        _extraStartingTokenChance = Mathf.Clamp(_extraStartingTokenChance, 0, 100);
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
