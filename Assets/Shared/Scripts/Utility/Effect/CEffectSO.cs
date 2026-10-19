using UnityEngine;

[CreateAssetMenu(menuName = "Data/Effect/EffectData", fileName = "EffectSO_")]
public class CEffectSO : ScriptableObject
{
    #region Inspector
    [Header("Basic Info")]
    [SerializeField] private string _name = "추잡한 식탐";
    [SerializeField] private EEffectType _effectType;

    [Header("Values")]
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _duration = 3;

    // 토큰 및 상태이상 부여 기본 확률
    [SerializeField] private int _baseChance = 100;

    // 부여하거나 상호작용할 토큰 종류
    [SerializeField] private EEffectType _tokenType = EEffectType.None;

    // 특정 발동 조건 (ex : 콤보 토큰 보유 시)
    [SerializeField] private string _condition = "None";

    // 효과가 발동될 때 실행될 실제 로직
    [Header("Strategy")]
    [SerializeField] private CEffectStrategy _strategy;
    #endregion

    #region Property
    public string Name => _name;
    public EEffectType EffectType => _effectType;
    public int Damage => _damage;
    public int Duration => _duration;

    public int BaseChance => _baseChance;
    public EEffectType TokenType => _tokenType;
    public string Condition => _condition;

    public CEffectStrategy Strategy => _strategy;
    #endregion
}
