using UnityEngine;

/// <summary>
/// 개별 이펙트 타겟팅
/// </summary>
public enum EEffectTarget
{
    Target,
    Self
}

/// <summary>
/// 특정 조건 제한
/// </summary>
public enum EEffectCondition
{
    None = 0,
    TargetHasCombo,
    OnCriticalHit,
    CasterIsDeathsDoor
}

[CreateAssetMenu(menuName = "Data/Effect/EffectData", fileName = "EffectSO_")]
public class CEffectSO : ScriptableObject
{
    #region Inspector
    [Header("Basic Info")]
    [SerializeField] private string _name = "추잡한 식탐";
    [SerializeField] private EEffectType _effectType;
    [SerializeField] private EEffectTarget _effectTarget = EEffectTarget.Target;

    [Header("Values")]
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _maxDamage = 0;
    [SerializeField] private int _duration = 3;

    // 토큰 및 상태이상 부여 기본 확률
    [SerializeField] private int _baseChance = 100;

    // 부여하거나 상호작용할 토큰 종류
    [SerializeField] private EEffectType _tokenType = EEffectType.None;

    // 특정 발동 조건 (ex : 콤보 토큰 보유 시)
    [SerializeField] private EEffectCondition _condition = EEffectCondition.None;

    // 효과가 발동될 때 실행될 실제 로직
    [Header("Strategy")]
    [SerializeField] private CEffectStrategy _strategy;
    #endregion

    #region Property
    public string Name => _name;
    public EEffectType EffectType => _effectType;
    public EEffectTarget EffectTarget => _effectTarget;
    public int Damage => _damage;
    public int MaxDamage => _maxDamage > _damage ? _maxDamage : _damage;
    public int Duration => _duration;

    public int BaseChance => _baseChance;
    public EEffectType TokenType => _tokenType;
    public EEffectCondition Condition => _condition;

    public CEffectStrategy Strategy => _strategy;
    #endregion

    /// <summary>
    /// 최소 ~ 최대 데미지 난수
    /// </summary>
    public int RollValue(bool isCrit = false)
    {
        if (isCrit || _maxDamage <= _damage)
        {
            return MaxDamage;
        }
        return Random.Range(_damage, _maxDamage + 1);
    }
}
