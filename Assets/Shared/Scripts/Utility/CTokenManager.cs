using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 엔티티가 보유한 토큰의 스택 관리하는 컴포넌트
/// </summary>
public class CTokenManager
{
    // 기본 토큰 최대 중첩 수치 (변동 가능)
    private const int Max_Token_STACK = 3;
    private const int MAX_COMBO_STACK = 1;
    private const int DEFAULT_TOKEN_DURATION = 2;

    // 내가 가지고 있는 토큰들 관리하는 딕셔너리
    // Key : EEffectType, Value : Stack
    private Dictionary<EEffectType, int> _tokenStacks = new Dictionary<EEffectType, int>();

    private Dictionary<EEffectType, int> _tokenDurations = new Dictionary<EEffectType, int>();

    /// <summary>
    /// UI 갱신용
    /// 스택이나 토큰이 0이 될시 (TOKENTYPE, 0, 0) 되도록
    /// </summary>
    public event Action<EEffectType, int, int> OnTokenChanged;

    /// <summary>
    /// 새로운 토큰  흭득시 호출
    /// </summary>
    public void AddToken(EEffectType tokenType, int amount = 1, int duration = 0)
    {
        EEffectType oppositeToken = GetOppositeToken(tokenType);
        if (oppositeToken != EEffectType.None)
        {
            while (amount > 0 && HasToken(oppositeToken))
            {
                ConsumeToken(oppositeToken);
                amount--;
                CPrint.Log($"[Token] 새로 들어온 [{tokenType}] 토큰이 기존 [{oppositeToken}] 토큰과 상쇄되어 소멸합니다.");
            }
        }

        if (amount <= 0) return;

        if (!_tokenStacks.ContainsKey(tokenType))
        {
            _tokenStacks[tokenType] = 0;
        }

        _tokenStacks[tokenType] += amount;

        // 토큰별 최대 중첩 수 제한
        int maxStack = GetMaxStackLimit(tokenType);
        if (_tokenStacks[tokenType] > maxStack)
        {
            _tokenStacks[tokenType] = maxStack;
        }

        int targetDuration = duration > 0 ? duration : DEFAULT_TOKEN_DURATION;
        if (!_tokenDurations.ContainsKey(tokenType) || _tokenDurations[tokenType] < targetDuration)
        {
            _tokenDurations[tokenType] = targetDuration;
        }

        int finalStack = _tokenStacks[tokenType];
        int finalTurn = _tokenDurations[tokenType];

        CPrint.Log($"[Token 흭득] {GetFormattedTooltip(tokenType)}");
        OnTokenChanged?.Invoke(tokenType, finalStack, finalTurn);
    }

    /// <summary>
    /// 특정 효과 발동 조건이 되었을시 토큰 1개 소모
    /// </summary>
    /// 소모에 성공시 true (효과 발동), 없을시 false (효과 무시)
    public bool ConsumeToken(EEffectType tokenType)
    {
        if (!_tokenStacks.ContainsKey(tokenType) || _tokenStacks[tokenType] <= 0)
        {
            return false;
        }

        _tokenStacks[tokenType]--;

        if (_tokenStacks[tokenType] <= 0 )
        {
            _tokenStacks.Remove(tokenType);
            _tokenDurations.Remove(tokenType);
            CPrint.Log($"[Token 소멸] [{tokenType}] 토큰 모두 소모");
            OnTokenChanged?.Invoke(tokenType, 0, 0);
        }
        else
        {
            int remainStack = _tokenStacks[tokenType];
            int remainTurn = GetTokenDuration(tokenType);
            CPrint.Log($"[Token 1회 소모] [{tokenType}] 남은 스택 : ({remainStack}), 남은 턴: ({remainTurn}턴)");
            OnTokenChanged?.Invoke(tokenType, remainStack, remainTurn);
        }

        return true;
    }

    /// <summary>
    /// 캐릭터 턴이 돌아오거나 끝날 때 호출해 남은 턴 차감
    /// </summary>
    public void TickTokenTurns()
    {
        if (_tokenDurations.Count == 0) return;

        List<EEffectType> activeTypes = new List<EEffectType>(_tokenDurations.Keys);
        foreach (EEffectType type in activeTypes)
        {
            if (type == EEffectType.Stun) continue;

            _tokenDurations[type]--;

            if (_tokenDurations[type] <= 0 )
            {
                _tokenDurations.Remove(type);
                _tokenStacks.Remove(type);
                CPrint.Log($"[Token 턴 만료] [{type}] 유지 시간이 끝나 소멸하였습니다.");
                OnTokenChanged?.Invoke(type, 0, 0);
            }
            else
            {
                int currentStack = GetTokenStack(type);
                int currentTurn = _tokenDurations[type];
                CPrint.Log($"[Token 턴 감소] [{type}] 현재 스택 : ({currentStack}), 남은 턴: ({currentTurn}턴)");
                OnTokenChanged?.Invoke(type, currentStack, currentTurn);
            }
        }
    }

    /// <summary>
    /// 현재 토큰 보유하고 있는지 확인용도
    /// ex : AI 판단이나 스킬 발동 조건 체크용으로 사용할 예정
    /// </summary>
    public bool HasToken(EEffectType tokenType)
    {
        return _tokenStacks.ContainsKey(tokenType) && _tokenStacks[tokenType] > 0;
    }

    /// <summary>
    /// 모든 토큰을 날릴 시 사용
    /// </summary>
    public void ClearAllTokens()
    {
        List<EEffectType> keys = new List<EEffectType>(_tokenStacks.Keys);
        _tokenStacks.Clear();
        _tokenDurations.Clear();

        foreach (EEffectType type in keys)
        {
            OnTokenChanged?.Invoke(type, 0, 0);
        }
    }

    /// <summary>
    /// 토큰 종류별 최대 몇 개 까지 가질 수 있는지
    /// </summary>
    private int GetMaxStackLimit(EEffectType tokenType)
    {
        switch (tokenType)
        {
            case EEffectType.Combo:
            case EEffectType.DenialOfArms:
            case EEffectType.DenialOfReach:
            case EEffectType.DenialOfFortitude:
            case EEffectType.DenialOfReason:
                return MAX_COMBO_STACK; // 1스택 제한
            default:
                return Max_Token_STACK; // 나머지는 3개
        }
    }

    private EEffectType GetOppositeToken(EEffectType tokenType)
    {
        switch (tokenType)
        {
            case EEffectType.Strength: return EEffectType.Weak;
            case EEffectType.Weak: return EEffectType.Strength;
            case EEffectType.Block:
            case EEffectType.BlockPlus: return EEffectType.Vulnerable;
            case EEffectType.Vulnerable: return EEffectType.Block;
            default:                    return EEffectType.None;
        }
    }

    private static readonly EEffectType[] _positiveTokens = new EEffectType[]
    {
        EEffectType.Block, EEffectType.BlockPlus,
        EEffectType.Dodge, EEffectType.DodgePlus,
        EEffectType.Strength, EEffectType.Crit,
        EEffectType.Speed, EEffectType.Taunt,
        EEffectType.Riposte, EEffectType.Stealth,
        EEffectType.Guard
    };

    private static readonly EEffectType[] _negativeTokens = new EEffectType[]
    {
        EEffectType.Blind, EEffectType.Weak,
        EEffectType.Vulnerable, EEffectType.Stun,
        EEffectType.Combo, EEffectType.Immobilize,
        EEffectType.DenialOfArms, EEffectType.DenialOfReach,
        EEffectType.DenialOfFortitude, EEffectType.DenialOfReason
    };

    /// <summary>
    /// 특정 토큰 스택 제거
    /// </summary>
    public int RemoveTokenAll(EEffectType tokenType)
    {
        if (_tokenStacks.TryGetValue(tokenType, out int count) && count > 0)
        {
            _tokenStacks.Remove(tokenType);
            _tokenDurations.Remove(tokenType);
            CPrint.Log($"[Token 제거] [{tokenType}] 토큰 {count}스택 제거됨");
            OnTokenChanged?.Invoke(tokenType, 0, 0);
            return count;
        }
        return 0;
    }

    public int RemoveTokensByCategory(bool removePositive)
    {
        EEffectType[] targetList = removePositive ? _positiveTokens : _negativeTokens;
        int totalRemoved = 0;

        for (int i = 0; i < targetList.Length; i++)
        {
            totalRemoved += RemoveTokenAll(targetList[i]);
        }
        return totalRemoved;
    }

    public int StealPositiveTokensTo(CTokenManager receiver, int maxTypesCount = 2)
    {
        if (receiver == null) return 0;

        int stolenTypes = 0;
        for (int i = 0; i < _positiveTokens.Length; i++)
        {
            if (stolenTypes >= maxTypesCount) break;

            EEffectType type = _positiveTokens[i];
            if (_tokenStacks.TryGetValue(type, out int stackCount) && stackCount > 0)
            {
                int remainTurn = GetTokenDuration(type);
                RemoveTokenAll(type);
                receiver.AddToken(type, stackCount, remainTurn);
                stolenTypes++;
            }
        }
        return stolenTypes;
    }

    /// <summary>
    /// 보유중인 모든 토큰(Combo 제외) 소모하고, 소모된 총 토큰 개수 반환
    /// </summary>
    public int ConsumeAllTokensCount()
    {
        int totalConsumed = 0;
        List<EEffectType> keys = new List<EEffectType>(_tokenStacks.Keys);
        
        for (int i = 0; i < keys.Count; i++)
        {
            EEffectType type = keys[i];
            if (type == EEffectType.Combo) continue;

            int count = _tokenStacks[type];
            if (count > 0)
            {
                totalConsumed += count;
                _tokenStacks.Remove(type);
            }
        }

        if (totalConsumed > 0)
        {
            CPrint.Log($"[Token] 보유 토큰 총 {totalConsumed}개를 소모했습니다.");
        }

        return totalConsumed;
    }

    /// <summary>
    /// [UI 연동용] 현재 남은 발동 횟수
    /// </summary>
    public int GetTokenStack(EEffectType tokenType)
    {
        return _tokenStacks.TryGetValue(tokenType, out int count) ? count : 0;
    }

    /// <summary>
    /// [UI 연동용] 현재 남은 유지 턴 수
    /// </summary>
    public int GetTokenDuration(EEffectType tokenType)
    {
        return (_tokenDurations.TryGetValue(tokenType, out int turns) && HasToken(tokenType) ? turns : 0);
    }

    public string GetFormattedTooltip(EEffectType tokenType)
    {
        int stack = GetTokenStack(tokenType);
        int turn = GetTokenDuration(tokenType);
        if (stack <= 0) return string.Empty;

        GetTokenDisplayText(tokenType, out string korName, out string desc);
        return $"{korName} ({stack}): {desc} ({turn}턴)";
    }

    private void GetTokenDisplayText(EEffectType tokenType, out string korName, out string desc)
    {
        switch (tokenType)
        {
            case EEffectType.Dodge: korName = "회피"; desc = "50% 확률로 다음 공격을 회피"; break;
            case EEffectType.DodgePlus: korName = "회피+"; desc = "75% 확률로 다음 공격을 회피"; break;
            case EEffectType.Block: korName = "방어"; desc = "다음 공격으로 받는 피해 -50%"; break;
            case EEffectType.BlockPlus: korName = "방어+"; desc = "다음 공격으로 받는 피해 -75%"; break;
            case EEffectType.Strength: korName = "힘"; desc = "다음 공격 피해량 +50%"; break;
            case EEffectType.Weak: korName = "약화"; desc = "다음 공격 피해량 -50%"; break;
            case EEffectType.Crit: korName = "치명타"; desc = "다음 공격 시 치명타 확률 100%"; break;
            case EEffectType.Vulnerable: korName = "취약"; desc = "다음 공격으로 받는 피해 +50%"; break;
            case EEffectType.Blind: korName = "실명"; desc = "50% 확률로 다음 공격 빗나감"; break;
            case EEffectType.Speed: korName = "속도"; desc = "다음 라운드 행동 속도 +3"; break;
            case EEffectType.Taunt: korName = "도발"; desc = "적에게 공격 대상으로 강제 지정됨"; break;
            case EEffectType.Riposte: korName = "반격"; desc = "피격 시 공격자에게 반격 피해"; break;
            case EEffectType.Stealth: korName = "은신"; desc = "적의 단일 공격 대상에서 제외됨"; break;
            case EEffectType.Guard: korName = "수호"; desc = "아군이 다음 공격을 대신 맞아줌"; break;
            case EEffectType.Combo: korName = "연계"; desc = "특정 스킬 피격 시 추가 효과 발동"; break;
            case EEffectType.Stun: korName = "기절"; desc = "다음 턴 행동 불가"; break;
            case EEffectType.Immobilize: korName = "이동불가"; desc = "열 이동 및 밀치기/당기기 면역"; break;
            case EEffectType.DenialOfArms: korName = "무장 부정"; desc = "근접 공격 스킬 사용 금지"; break;
            case EEffectType.DenialOfReach: korName = "도달 부정"; desc = "원거리 공격 스킬 사용 금지"; break;
            case EEffectType.DenialOfFortitude: korName = "불굴 부정"; desc = "체력 치유 스킬 사용 금지"; break;
            case EEffectType.DenialOfReason: korName = "이성 부정"; desc = "스트레스 치유 스킬 사용 금지"; break;
            default: korName = tokenType.ToString(); desc = "특수 상태 토큰"; break;
        }
    }
}
