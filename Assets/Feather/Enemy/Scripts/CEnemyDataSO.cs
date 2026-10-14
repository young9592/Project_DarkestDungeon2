using UnityEngine;

[CreateAssetMenu(menuName = "Data/Entity/Enemy", fileName = "EnemySO_")]
public class CEnemyDataSO : CEntityDataSO
{
    #region Inspector
    [Header("DeathDoorCount")]
    [SerializeField] private int _deathDoorCount = 0;
    #endregion

    #region Property
    public int DeathDoorCount => _deathDoorCount;
    #endregion

    protected override void OnValidate()
    {
        base.OnValidate();

        _deathDoorCount = Mathf.Max(0, _deathDoorCount);
    }
}
