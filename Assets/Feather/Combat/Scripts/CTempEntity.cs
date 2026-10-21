using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CTempEntity : CEntity
{
    // 추가행동 악세사리 착용여부
    public enum EJewelryType
    {
        None,
        TurnStart,
        TurnEnd,
    }

    #region Inspector
    [Header("Entity Inspector")]
    [SerializeField] private EEntityType _type;
    // 행동권 : 1회 기본 / 2회 ~ 3회 보스
    [SerializeField] private int _actions = 1;

    [Header("Test State")]
    // 임시 값
    [SerializeField] private int _currentHealth = 10;
    [SerializeField] private int _speed;
    [SerializeField] private bool _isDead = false;
    // 추가 행동 부여
    [SerializeField] private bool _hasExtraTurn = false;
    [SerializeField] private EJewelryType _equipJewelry = EJewelryType.None;
    #endregion

    #region Field
    private List<CEffect> _dotEffectList = new List<CEffect>();
    #endregion

    #region Property
    public EEntityType Type => _type;
    public float Actions => _actions;
    public bool HasExtraTurn { get; set; }
    public EJewelryType EquipJewelry => _equipJewelry;
    public int TempEntitySpeed => _speed;
    public bool IsDead => _isDead;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    // 행동
    public bool Action()
    {
        if (_type == EEntityType.Hero)
        {
            if (!Keyboard.current.aKey.isPressed)
            {
                return false;
            }
        }

        // 행동 성공
        return true;
    }
    // 데미지 적용
    public override void TakeDamage(int finalDamage)
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth -= finalDamage;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            _isDead = true;
        }
    }
    // 상태이상 적용
    public override void ApplyStatusEffect(CEffect statusEffect)
    {
        EEffectType eEffectType = statusEffect.EffectType;

        switch (eEffectType)
        {
            case EEffectType.Bleed:
            case EEffectType.Blight:
            case EEffectType.Burn:
                _dotEffectList.Add(statusEffect);
                break;
            case EEffectType.Daze:
            case EEffectType.Stun:
                TokenManager.AddToken(eEffectType);
                break;
        }
    }
    // 임시 도트 데미지 소모
    public (int, int, int) ConsumeDOTEffect()
    {
        (int bleed, int blight, int burn) total = (0, 0, 0);

        if(_dotEffectList.Count == 0)
        {
            return total;
        }

        for (int i = 0; i < _dotEffectList.Count; i++)
        {
            if (_dotEffectList[i].RemainingTurns <= 0)
            {
                _dotEffectList.RemoveAt(i);
                i -= 1;
                continue;
            }

            _dotEffectList[i].DecreaseTurn();
            switch (_dotEffectList[i].EffectType)
            {
                case EEffectType.Bleed:
                    total.bleed += _dotEffectList[i].Damage;
                    break;
                case EEffectType.Blight:
                    total.blight += _dotEffectList[i].Damage;
                    break;
                case EEffectType.Burn:
                    total.burn += _dotEffectList[i].Damage;
                    break;
            }

            Debug.Log($"Dot[{_dotEffectList[i].EffectType}] : {_dotEffectList[i].Damage} / {_dotEffectList[i].RemainingTurns}");
        }

        TakeDamage(total.bleed + total.blight + total.burn);

        return total;
    }
    // 임시 토큰 소모
    public bool ConsumeTokenEffect(EEffectType eEffectType)
    {
        bool consumeToken = TokenManager.ConsumeToken(eEffectType);

        if (consumeToken)
        {
            Debug.Log($"Token[{eEffectType}] Use");
        }

        return consumeToken;
    }
}
