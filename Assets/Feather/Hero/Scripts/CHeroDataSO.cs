
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Entity/Hero", fileName = "HeroSO_")]
public class CHeroDataSO : CEntityDataSO
{
    #region Inspector
    [Header("Info")]
    [SerializeField] private string _defaultName = "바리스탄";
    // 영웅한테만 있는 저항력입니다.
    [Header("Hero_Resistance")]
    [SerializeField] private int _diseaseResistance = 30;
    [SerializeField] private int _deathDoorResistance = 60;
    [SerializeField] private int _stressResistance = 0;

    [Header("Hero_Stress")]
    [SerializeField] private int _maxStress = 10;

    // 플레이어가 임의로 전진/후진 할 수 있습니다.
    [Header("Movement")]
    [SerializeField] private int _forwardMovement = 2;
    [SerializeField] private int _backwardMovement = 2;

    [Header("Skills")]
    [SerializeField] private List <CSkillDataSO> _availableSkills = new List<CSkillDataSO>();
    #endregion

    #region Property
    public string DefaultName => _defaultName;
    public int DiseaseResistance => _diseaseResistance;
    public int DeathDoorResistance => _deathDoorResistance;
    public int StressResistance => _stressResistance;
    public int MaxStress => _maxStress;
    public int ForwardMovement => _forwardMovement;
    public int BackwardMovement => _backwardMovement;
    public IReadOnlyList<CSkillDataSO> AvailableSkills => _availableSkills;
    #endregion

    protected override void OnValidate()
    {
        base.OnValidate();

        _deathDoorResistance = Mathf.Max(0, _deathDoorResistance);
        _diseaseResistance = Mathf.Max(0, _diseaseResistance);
        _stressResistance = Mathf.Max(0, _stressResistance);
        _maxStress = Mathf.Max(1, _maxStress);
        _forwardMovement = Mathf.Max(0, _forwardMovement);
        _backwardMovement = Mathf.Max(0, _backwardMovement);
    }
}
