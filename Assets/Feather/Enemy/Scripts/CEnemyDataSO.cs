using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Entity/Enemy", fileName = "EnemySO_")]
public class CEnemyDataSO : CEntityDataSO
{
    #region Inspector
    [Header("DeathDoorCount")]
    [SerializeField] private int _deathDoorCount = 0;

    [Header("Skills")]
    [SerializeField] private List<CSkillDataSO> _availableSkills = new List<CSkillDataSO>();
    #endregion

    #region Property
    public int DeathDoorCount => _deathDoorCount;

    public IReadOnlyList<CSkillDataSO> AvailableSkills
    {
        get { return _availableSkills; }
    }
    #endregion

    protected override void OnValidate()
    {
        base.OnValidate();

        _deathDoorCount = Mathf.Max(0, _deathDoorCount);
    }
}
