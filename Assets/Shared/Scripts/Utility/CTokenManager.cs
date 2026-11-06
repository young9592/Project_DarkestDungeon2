using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 엔티티가 보유한 토큰의 스택 관리하는 컴포넌트
/// </summary>
public class CTokenManager
{
    // 기본 토큰 최대 중첩 수치 (변동 가능)
    private const int Max_Token_STACK = 2;
    private const int MAX_COMBO_STACK = 1;

    // 내가 가지고 있는 토큰들 관리하는 딕셔너리
    // Key : EEffectType, Value : Stack
    private Dictionary<EEffectType, int> _tokenStacks = new Dictionary<EEffectType, int>();

    /// <summary>
    /// 새로운 토큰 득시 호출
    /// </summary>
    public void AddToken(EEffectType tokenType, int amount = 1)
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

        CPrint.Log($"[Token] {tokenType} 토큰 득! (현재 스택 : {_tokenStacks[tokenType]}");

        // TODO : UI 매니저에 이벤트를 보내 토큰 득 애니메이션 + 아이콘 갱신
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

        CPrint.Log($"[Token] {tokenType} 토큰 소모! (남은 스택 : {_tokenStacks[tokenType]}");

        // TODO : UI 매니저에 이벤트 보내 토큰 깨지는 애니메이션 및 아이콘 추가

        return true;
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
        _tokenStacks.Clear();
        CPrint.Log($"[Token] 모든 토큰이 초기화되었습니다.");
    }

    /// <summary>
    /// 토큰 종류별 최대 몇 개 까지 가질 수 있는지
    /// </summary>
    private int GetMaxStackLimit(EEffectType tokenType)
    {
        switch (tokenType)
        {
            case EEffectType.Combo:
                return MAX_COMBO_STACK; // 콤보는 1개
            default:
                return Max_Token_STACK; // 나머지는 2개
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
        EEffectType.Speed, EEffectType.Taunt
    };

    private static readonly EEffectType[] _negativeTokens = new EEffectType[]
    {
        EEffectType.Blind, EEffectType.Weak,
        EEffectType.Vulnerable, EEffectType.Stun,
        EEffectType.Combo
    };

    /// <summary>
    /// 특정 토큰 스택 제거
    /// </summary>
    public int RemoveTokenAll(EEffectType tokenType)
    {
        if (_tokenStacks.TryGetValue(tokenType, out int count) && count > 0)
        {
            _tokenStacks.Remove(tokenType);
            CPrint.Log($"[Token] [{tokenType}] 토큰 {count}개 제거됨");
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
            if (_tokenStacks.TryGetValue(type, out int count) && count > 0)
            {
                _tokenStacks.Remove(type);
                receiver.AddToken(type, count);
                stolenTypes++;
                CPrint.Log($"[Token 강탈] [{type}] 토큰 {count}개를 강탈하였습니다");
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
}
