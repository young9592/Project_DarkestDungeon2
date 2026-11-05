using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/Effect/DamageStrategy", fileName = "DamageStrategy_")]
public class CDamageStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Caster == null || context.Target == null) return;

        CEntity attacker = context.Caster;
        CEntity defender = context.Target;

        if (defender.TokenManager.HasToken(EEffectType.Guard))
        {
            CEntity protector = defender.GuardProtector;
            if (protector != null && !protector.IsDead && !protector.TokenManager.HasToken(EEffectType.Stun))
            {
                defender.TokenManager.ConsumeToken(EEffectType.Guard);
                CPrint.Log($"[수호 발동] [{protector.name}] 가 [{defender.name}] 대신 공격을 맞습니다");
                defender = protector;
            }
            else
            {
                defender.SetGuardProtector(null);
            }
        }

        // 1. 명중 / 회피 판정 (토큰 Intercept)
        // 공격자가 실명 토큰 보유시 50% 빗나가기
        if (context.ConsumeCasterTokenOnce(EEffectType.Blind))
        {
            CPrint.Log($"[{attacker.name}] 의 [실명] 토큰 소모");
            if (Random.Range(1, 101) <= 50)
            {
                CPrint.Log($"[{attacker.name}] 의 공격이 [{defender.name}]에게서 빗나갔습니다.");
                context.IsHit = false;
                TryTriggerRiposte(defender, attacker);
                return;
            }
        }
        
        // 방어자가 회피+ or 회피 토큰 보유시
        if (defender.TokenManager.ConsumeToken(EEffectType.DodgePlus))
        {
            CPrint.Log($"[{defender.name}] 의 [회피+] 토큰 소모");
            if (Random.Range(1, 101) <= 75)
            {
                CPrint.Log($"[{defender.name}] 의 [회피+] 토큰으로 공격을 회피했습니다.");
                context.IsHit = false;
                TryTriggerRiposte(defender, attacker);
                return;
            }
        }
        else if (defender.TokenManager.ConsumeToken(EEffectType.Dodge))
        {
            CPrint.Log($"[{defender.name}] 의 [회피] 토큰 소모");
            if (Random.Range(1, 101) <= 75)
            {
                CPrint.Log($"[{defender.name}] 의 [회피] 토큰으로 공격을 회피했습니다.");
                context.IsHit = false;
                TryTriggerRiposte(defender, attacker);
                return;
            }
        }

        // 크리티컬 판정 연산
        if (context.ConsumeCasterTokenOnce(EEffectType.Crit))
        {
            context.IsCrit = true;
            CPrint.Log($"[{attacker.name}] 의 [치명타] 토큰 소모");
        }
        else if (Random.Range(1, 101) <= 5)
        {
            context.IsCrit = true;
            CPrint.Log($"[{attacker.name}] 기본 확률 치명타 발동");
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
        }

       // 4. 공격자 및 방어자의 토큰 연산 (파이프라인 : Intercept)
       // 공격자가 힘 토큰 가지고 있을때
       if (context.ConsumeCasterTokenOnce(EEffectType.Strength))
        {
            finalDamage *= 1.5f;
            CPrint.Log($"[{attacker.name}] 의 [힘] 토큰 소모 \n (데미지 50%)");
        }

        if (context.ConsumeCasterTokenOnce(EEffectType.Weak))
        {
            finalDamage *= 0.5f;
            CPrint.Log($"[{attacker.name}] 의 [약화] 토큰 소모 \n (데미지 -50%)");
        }

        // 방어 토큰 가지고 있을때
        if (defender.TokenManager.ConsumeToken(EEffectType.BlockPlus))
        {
            finalDamage *= 0.25f;
            CPrint.Log($"[{defender.name}] 의 [방어+] 토큰 소모 \n (받는 피해 -75%)");
        }
        else if (defender.TokenManager.ConsumeToken(EEffectType.Block))
        {
            finalDamage *= 0.5f;
            CPrint.Log($"[{defender.name}] 의 [방어] 토큰 소모 \n (받는 피해 -50%)");
        }

        // 취약 토큰 가지고 있을때
        if (defender.TokenManager.ConsumeToken(EEffectType.Vulnerable))
        {
            finalDamage *= 1.5f;
            CPrint.Log($"[{defender.name}] 의 [취약] 토큰 소모 \n (받는 피해 +50%)");
        }

        // 5. 최종 산출 된 수치를 정수로 변환시켜 대상의 HP 반영
        int calculatedDamage = Mathf.RoundToInt(finalDamage);
        CPrint.Log($"[{attacker.name}]가 [{defender.name}]에게 최종 데미지 [{calculatedDamage}]를 입혔습니다.");
        // 계산이 다 끝난 최종 데미지 수치만 전달받음
        defender.TakeDamage(calculatedDamage);

        TryTriggerRiposte(defender, attacker);
    }

    private void TryTriggerRiposte(CEntity defender, CEntity attacker)
    {
        if (defender == null || attacker == null || defender == attacker) return;
        if (defender.IsDead || attacker.IsDead) return;
        if (defender.TokenManager.HasToken(EEffectType.Stun)) return;

        if (defender.TokenManager.ConsumeToken(EEffectType.Riposte))
        {
            int riposteDamage = Random.Range(3, 6);
            CPrint.Log($"[반격] [{defender.name}]가 공격자인 [{attacker.name}]에게 [{riposteDamage}] 반격 피해를 입힙니다");
            attacker.TakeDamage(riposteDamage);
        }
    }
}
