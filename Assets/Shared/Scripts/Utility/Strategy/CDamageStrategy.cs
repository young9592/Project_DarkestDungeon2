using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/DamageStrategy", fileName = "DamageStrategy_")]
public class CDamageStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        // 1. 명중 / 회피 판정 (토큰 Intercept)
        // 공격자가 실명 토큰 보유시 50% 빗나가기
        if (context.ConsumeCasterTokenOnce(EEffectType.Blind))
        {
            CPrint.Log($"[{context.Caster.name}] 의 [실명] 토큰 소모");
            if (Random.Range(1, 101) <= 50)
            {
                CPrint.Log($"[{context.Caster.name}] 의 공격이 [{context.Target.name}]에게서 빗나갔습니다.");
                context.IsHit = false;
                return;
            }
        }
        
        // 방어자가 회피+ or 회피 토큰 보유시
        if (context.Target.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            CPrint.Log($"[{context.Target.name}] 의 [회피+] 토큰 소모");
            if (Random.Range(1, 101) <= 75)
            {
                CPrint.Log($"[{context.Target.name}] 의 [회피+] 토큰으로 공격을 회피했습니다.");
                context.IsHit = false;
                return;
            }
        }
        else if (context.Target.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            CPrint.Log($"[{context.Target.name}] 의 [회피] 토큰 소모");
            if (Random.Range(1, 101) <= 75)
            {
                CPrint.Log($"[{context.Target.name}] 의 [회피] 토큰으로 공격을 회피했습니다.");
                context.IsHit = false;
                return;
            }
        }

        // 크리티컬 판정 연산
        if (context.ConsumeCasterTokenOnce(EEffectType.Crit))
        {
            context.IsCrit = true;
            CPrint.Log($"[{context.Caster.name}] 의 [치명타] 토큰 소모");
        }
        else
        {
            int baseCrit = 5;
            if (Random.Range(1, 101) <= baseCrit)
            {
                context.IsCrit = true;
                CPrint.Log($"[{context.Caster.name}] 기본 확률 치명타 발동");
            }
        }

        // 2. Base 롤링
        // TODO : 데미지의 Min / Max가 필요해질시 CEffectSO 수정해야 할 수도..
        int baseDamage = effectData.RollValue(context.IsCrit);
        float finalDamage = baseDamage;

        CPrint.Log($"[기본데미지] : Base 롤링 데미지 = {baseDamage}");

       if (context.IsCrit)
        {
            // 크리티컬 발생시 데미지 (수치 조절 필요)
            finalDamage *= 1.5f;
            CPrint.Log($"[기본데미지] : 크리티컬 적용! 데미지가 증가합니다.");
        }

       // 4. 공격자 및 방어자의 토큰 연산 (파이프라인 : Intercept)
       // 공격자가 힘 토큰 가지고 있을때
       if (context.ConsumeCasterTokenOnce(EEffectType.Strength))
        {
            finalDamage *= 1.5f;
            CPrint.Log($"[{context.Caster.name}] 의 [힘] 토큰 소모 \n (데미지 50%)");
        }

        if (context.ConsumeCasterTokenOnce(EEffectType.Weak))
        {
            finalDamage *= 0.5f;
            CPrint.Log($"[{context.Caster.name}] 의 [약화] 토큰 소모 \n (데미지 -50%)");
        }

        // 방어 토큰 가지고 있을때
        if (context.Target.TokenManager.ConsumeToken(EEffectType.BlockPlus))
        {
            finalDamage *= 0.25f;
            CPrint.Log($"[{context.Target.name}] 의 [방어+] 토큰 소모 \n (받는 피해 -75%)");
        }
        else if (context.Target.TokenManager.ConsumeToken(EEffectType.Block))
        {
            finalDamage *= 0.5f;
            CPrint.Log($"[{context.Target.name}] 의 [방어] 토큰 소모 \n (받는 피해 -50%)");
        }

        // 취약 토큰 가지고 있을때
        if (context.Target.TokenManager.ConsumeToken(EEffectType.Vulnerable))
        {
            finalDamage *= 1.5f;
            CPrint.Log($"[{context.Target.name}] 의 [취약] 토큰 소모 \n (받는 피해 +50%)");
        }

        // 5. 최종 산출 된 수치를 정수로 변환시켜 대상의 HP 반영
        int calculatedDamage = Mathf.RoundToInt(finalDamage);
        CPrint.Log($"[{context.Caster.name}]가 [{context.Target.name}]에게 최종 데미지 [{calculatedDamage}]를 입혔습니다.");
        
        // 계산이 다 끝난 최종 데미지 수치만 전달받음
        context.Target.TakeDamage(calculatedDamage);

    }
}
