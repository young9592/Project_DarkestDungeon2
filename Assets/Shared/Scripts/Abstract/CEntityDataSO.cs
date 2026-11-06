using System.Collections.Generic;
using UnityEngine;

public abstract class CEntityDataSO : ScriptableObject
{
    [System.Serializable]
    public struct CResistanceStats
    {
        // 출혈
        public int Bleed;
        // 중독
        public int Blight;
        // 화상
        public int Burn;
        // 현기증 [ 행동이 맨 뒤로 밀림, 2회 중첩시 기절로 변경 ], 기절 [ 행동이 다음 라운드로 밀림 ]
        public int Stun;
        // 이동저항 [ 적을 밀어버리는 기술 저항력 ]
        public int Move;
        // 약화토큰 종류 : 취약[받는 피해량 증가], 실명[다음공격 빗나갈 확률 증가], 약화[주는 피해량 감소], 속도 감소[행동 순서 밀림]
        public int Debuff;
    }

    #region Inspector
    [Header("Info")]
    [SerializeField] private string _id = "";
    [SerializeField] private string _name = "";

    [Header("Status")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private int _speed = 1;
    [SerializeField] private CResistanceStats _resistance;
    [SerializeField] private int _actions;   // 행동횟수


    [Header("Skills")]
    [SerializeField] private List<CSkillDataSO> _availableSkills = new List<CSkillDataSO>();

    [Header("Sprite")]
    [SerializeField] private Sprite _portraitTurnOrder;
    [SerializeField] private Sprite _portraitInfo;
    [SerializeField] private Sprite _skillSlotBackground;
    #endregion

    #region Property
    public string Id => _id;
    public string Name => _name;
    public int MaxHealth => _maxHealth;
    public int Speed => _speed;
    public CResistanceStats Resistance => _resistance;
    public int Actions => _actions;
    public IReadOnlyList<CSkillDataSO> AvailableSkills
    {
        get { return _availableSkills; }
    }

    public Sprite PortraitTurnOrder => _portraitTurnOrder;
    public Sprite PortraitInfo => _portraitInfo;
    public Sprite SkillSlotBackground => _skillSlotBackground;
    #endregion

    protected virtual void OnValidate()
    {
        if(string.IsNullOrWhiteSpace(_id))
        {
            CPrint.Warn($"{name} : id가 비어있습니다.");
        }

        if (string.IsNullOrWhiteSpace(_name))
        {
            CPrint.Warn($"{name} : 이름이 비어있습니다.");
        }

        _maxHealth = Mathf.Max(1, _maxHealth);
        _speed = Mathf.Max(1, _speed);
        _resistance.Bleed = Mathf.Max(0, _resistance.Bleed);
        _resistance.Blight = Mathf.Max(0, _resistance.Blight);
        _resistance.Burn = Mathf.Max(0, _resistance.Burn);
        _resistance.Stun = Mathf.Max(0, _resistance.Stun);
        _resistance.Move = Mathf.Max(0, _resistance.Move);
        _resistance.Debuff = Mathf.Max(0, _resistance.Debuff);
    }
}
