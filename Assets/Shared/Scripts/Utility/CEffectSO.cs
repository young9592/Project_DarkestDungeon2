using UnityEngine;

[CreateAssetMenu(menuName = "Data/Effect/EffectData", fileName = "EffectSO_")]
public class CEffectSO : ScriptableObject
{
    #region Inspector
    [SerializeField] private string _name = "추잡한 식탐";
    [SerializeField] private EEffectType _effectType;
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _duration = 3;

    // 효과가 발동될 때 실행될 실제 로직
    [SerializeField] private CEffectStrategy _strategy;
    #endregion

    #region Property
    public string Name => _name;
    public EEffectType EffectType => _effectType;
    public int Damage => _damage;
    public int Duration => _duration;
    public CEffectStrategy Strategy => _strategy;
    #endregion
}
