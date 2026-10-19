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
    /// 새로운 토큰 흭득시 호출
    /// </summary>
    public void AddToken(EEffectType tokenType, int amount = 1)
    {
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

        CPrint.Log($"[Token] {tokenType} 토큰 흭득! (현재 스택 : {_tokenStacks[tokenType]}");

        // TODO : UI 매니저에 이벤트를 보내 토큰 흭득 애니메이션 + 아이콘 갱신
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
}
