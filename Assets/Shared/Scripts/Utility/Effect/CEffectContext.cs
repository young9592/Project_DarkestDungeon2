using System.Collections.Generic;

/// <summary>
/// 스킬 발동 시 생성되어 데미지나 토큰 효과를 적용할 때 시전자와 대상, 명중 여부를 전달해주는 정보 컨테이너
/// 스킬 연산 중 Effect 간에 정보 공유를 위한 컨텍스트
/// </summary>

public class CEffectContext
{
    public CEntity Caster { get; private set; }
    public CEntity Target { get; private set; }

    // 회피 발동시 IsHit → false 처리
    public bool IsHit { get; set; }
    public bool IsCrit { get; set; }

    private readonly Dictionary<EEffectType, bool> _sharedCasterTokens;

    public CEffectContext(CEntity caster, CEntity target, Dictionary<EEffectType, bool> sharedCasterTokens = null)
    {
        Caster = caster;
        Target = target;
        IsHit = true;
        IsCrit = false;
        _sharedCasterTokens = sharedCasterTokens ?? new Dictionary<EEffectType, bool>();
    }

    /// <summary>
    /// 토큰을 스킬 실행 중 1번 소모하고, 광역 공격 나머지 대상들에게 동일한 토큰 발동 여부 반환시킴
    /// </summary>
    public bool ConsumeCasterTokenOnce(EEffectType tokenType)
    {
        if (Caster == null || Caster.TokenManager == null) return false;

        if (_sharedCasterTokens.TryGetValue(tokenType, out bool alreadConsumedResult))
        {
            return alreadConsumedResult;
        }

        bool consumed = Caster.TokenManager.ConsumeToken(tokenType);
        _sharedCasterTokens[tokenType] = consumed;
        return consumed;
    }
}
