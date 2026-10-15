using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/DamageStrategy", fileName = "DamageStrategy_")]
public class CDamageStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        // 1. 명중 확인
        if (!context.IsHit)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나갔습니다.");
            return;
        }

        // 2. Base 롤링
        // TODO : 데미지의 Min / Max가 필요해질시 CEffectSO 수정해야 할 수도..
        int baseDamage = effectData.Damage;
        float finalDamage = baseDamage;

        CPrint.Log($"[{effectData.Name}] : Base 롤링 데미지 = {baseDamage}");

        // 3. 크리티컬 판정 연산
       if (context.IsCrit)
        {
            // 크리티컬 발생시 데미지 (수치 조절 필요)
            finalDamage *= 1.2f;
            CPrint.Log($"[{effectData.Name}] : 크리티컬 적용! 데미지가 증가합니다.");
        }

       // 4. 공격자 및 방어자의 토큰 연산 (파이프라인 : Intercept)
       // TODO : TokenManager 구현 후, 사용 예정
       // ex : 공격자 힘 토큰 소모 데미지 몇 배 증가

       // 5. 최종 산출 된 수치를 정수로 변환시켜 대상의 HP 반영
       int calculatedDamage = Mathf.RoundToInt(finalDamage);

        // 계산이 다 끝난 최종 데미지 수치만 전달받음
        context.Target.TakeDamage(calculatedDamage);

        CPrint.Log($"[{context.Caster.name}]가 [{context.Target.name}]에게 최종 데미지 [{calculatedDamage}]를 입혔습니다.");
    }
}
