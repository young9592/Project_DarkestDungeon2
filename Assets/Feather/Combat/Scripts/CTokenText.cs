using TMPro;
using UnityEngine;

public class CTokenText : MonoBehaviour
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private TextMeshProUGUI _tokenText;
    #endregion

    // TODO : 보호받음 관련 정보 넘기기 필요
    public void SetTokenInfomation(EEffectType type, int duration, int remain)
    {
        string name = $"<color=#C98200>{SetTokenName(type)}</color> ";
        string desc = "";

        // 죽음의 문턱이나 죽음방지는 에외로 변경해야함
        if (type == EEffectType.DeathDoor)
        {
            desc = $"{SetTokenDesc(type)}";
        }
        else if (type == EEffectType.DeathArmor)
        {
            desc = $"{SetTokenDesc(type)}";
        }
        else if (type == EEffectType.DenialOfArms)
        {
            name = $"<color=#42556b>{SetTokenName(type)}</color> : ";
            desc = $"{SetTokenDesc(type)}";
        }
        else if (type == EEffectType.DenialOfReach)
        {
            name = $"<color=#42556b>{SetTokenName(type)}</color> : ";
            desc = $"{SetTokenDesc(type)}";
        }
        else if (type == EEffectType.DenialOfFortitude)
        {
            name = $"<color=#42556b>{SetTokenName(type)}</color> : ";
            desc = $"{SetTokenDesc(type)}";
        }
        else if (type == EEffectType.DenialOfReason)
        {
            name = $"<color=#42556b>{SetTokenName(type)}</color> : ";
            desc = $"{SetTokenDesc(type)}";
        }
        else
        {
            desc = $"({remain}): {SetTokenDesc(type)} ({duration}턴)";
        }

        _tokenText.text = name + desc;
    }

    // 수호 토큰 보호대상 이름 가져오기
    public void SetTokenInfomation(EEffectType type, int duration, int remain, string defender)
    {
        string name = $"<color=#C98200>{SetTokenName(type)}</color> ";
        string desc = $"({remain}): {SetTokenDesc(type, defender)} ({duration}턴)";
        _tokenText.text = name + desc;
    }

    private string SetTokenName(EEffectType type)
    {
        string name = "";

        switch (type)
        {
            case EEffectType.DeathDoor:
                name = "죽음의 문턱";
                break;
            case EEffectType.DeathArmor:
                name = "죽음 저항";
                break;
            case EEffectType.Crit:
                name = "치명타";
                break;
            case EEffectType.Riposte:
                name = "반격";
                break;
            case EEffectType.Dodge:
                name = "회피";
                break;
            case EEffectType.DodgePlus:
                name = "회피+";
                break;
            case EEffectType.Block:
                name = "방호";
                break;
            case EEffectType.BlockPlus:
                name = "방호+";
                break;
            case EEffectType.Guard:
                name = "보호";
                break;
            case EEffectType.Strength:
                name = "힘";
                break;
            case EEffectType.Speed:
                name = "속도";
                break;
            case EEffectType.Stealth:
                name = "은신";
                break;
            case EEffectType.Daze:
                name = "현기증";
                break;
            case EEffectType.Stun:
                name = "기절";
                break;
            case EEffectType.Blind:
                name = "실명";
                break;
            case EEffectType.Weak:
                name = "약화";
                break;
            case EEffectType.Vulnerable:
                name = "취약";
                break;
            case EEffectType.Taunt:
                name = "도발";
                break;
            case EEffectType.Immobilize:
                name = "이동 불가";
                break;
            case EEffectType.Combo:
                name = "연계";
                break;
            case EEffectType.DenialOfArms:
                name = "근접 잠금";
                break;
            case EEffectType.DenialOfReach:
                name = "원거리 잠금";
                break;
            case EEffectType.DenialOfFortitude:
                name = "체력 잠금";
                break;
            case EEffectType.DenialOfReason:
                name = "스트레스 잠금";
                break;
            default:
                break;
        }

        return name;
    }
    // 보호 토큰 전용
    private string SetTokenDesc(EEffectType type, string defender)
    {
        string text = "";
        switch (type)
        {
            case EEffectType.Guard:
                text = $"{defender}의 보호 받는중";
                break;
        }

        return text;
    }
    private string SetTokenDesc(EEffectType type)
    {
        string text = "";

        switch (type)
        {
            case EEffectType.DeathDoor:
                text = "죽음의 일격을 받을 수 있음";
                break;
            case EEffectType.DeathArmor:
                text = "죽음의 일격을 방지";
                break;
            case EEffectType.Crit:
                text = "다음 기술은 치명타 100% 발생";
                break;
            case EEffectType.Riposte:
                text = "공격자에게 반격 공격을 가함";
                break;
            case EEffectType.Dodge:
                text = "50% 확률로 다음 공격을 회피";
                break;
            case EEffectType.DodgePlus:
                text = "75% 확률로 다음 공격을 회피";
                break;
            case EEffectType.Block:
                text = "다음 공격으로 입는 피해 -50%";
                break;
            case EEffectType.BlockPlus:
                text = "다음 공격으로 입는 피해 -75%";
                break;
            case EEffectType.Guard:
                text = "OOO의 보호 받는중";
                break;
            case EEffectType.Strength:
                text = "다음 공격이 +50%피해를 입힘";
                break;
            case EEffectType.Speed:
                text = "다음 라운드에 더 빨리 행동";
                break;
            case EEffectType.Stealth:
                text = "직접 공격의 대상이 되지 않음";
                break;
            case EEffectType.Daze:
                text = "다음 행동이 라운드의 마지막으로 밀림";
                break;
            case EEffectType.Stun:
                text = "다음 행동을 상실";
                break;
            case EEffectType.Blind:
                text = "50% 확률로 공격이 빗나감";
                break;
            case EEffectType.Weak:
                text = "다음 공격이 -50%피해를 입힘";
                break;
            case EEffectType.Vulnerable:
                text = "다음 공격으로 입는 피해 +50%";
                break;
            case EEffectType.Taunt:
                text = "적의 공격 대상이 됨(가능할 경우)";
                break;
            case EEffectType.Immobilize:
                text = "이동하거나 이동될 수 없음";
                break;
            case EEffectType.Combo:
                text = "지정 시 추가 기술 효과 발동";
                break;
            case EEffectType.DenialOfArms:
                text = "근접 기술 사용 불가";
                break;

            case EEffectType.DenialOfReach:
                text = "원거리 기술 사용 불가";
                break;

            case EEffectType.DenialOfFortitude:
                text = "치유 기술이나 아이템 사용 불가";
                break;

            case EEffectType.DenialOfReason:
                text = "스트레스 치유 기술이나 아이템 사용 불가";
                break;
            default:
                break;

        }

        return text;
    }
}
