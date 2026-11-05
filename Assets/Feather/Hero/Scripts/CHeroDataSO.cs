
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Entity/Hero", fileName = "HeroSO_")]
public class CHeroDataSO : CEntityDataSO
{
    #region Inspector
    [Header("Info")]
    [SerializeField] private string _defaultName = "바리스탄";
    // 영웅 전용 저항력
    [Header("Hero_Resistance")]
    [SerializeField] private int _diseaseResistance = 30;
    [SerializeField] private int _deathDoorResistance = 60;
    [SerializeField] private int _stressResistance = 0;

    [Header("Hero_Stress")]
    [SerializeField] private int _maxStress = 10;

    // 플레이어가 직접 전진하거나 후퇴할 수 있음
    [Header("Movement")]
    [SerializeField] private int _forwardMovement = 2;
    [SerializeField] private int _backwardMovement = 2;
    #endregion

    #region Property
    public string DefaultName => _defaultName;
    public int DiseaseResistance => _diseaseResistance;
    public int DeathDoorResistance => _deathDoorResistance;
    public int StressResistance => _stressResistance;
    public int MaxStress => _maxStress;
    public int ForwardMovement => _forwardMovement;
    public int BackwardMovement => _backwardMovement;
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
