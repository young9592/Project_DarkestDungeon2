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

    public CEffectContext(CEntity caster, CEntity target)
    {
        Caster = caster;
        Target = target;
        IsHit = true;
        IsCrit = false;
    }
}
