using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

// 배틀코어에서 스킬 정보를 넘겨받아 Effect들을 순차적으로 실행하고 결과를 반환해주는 매니저
public class CEffectManager : MonoBehaviour
{
    private static CEffectManager _instance;
    public static CEffectManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// 연동용 진입점
    /// 타겟 지정이 끝날 시 이 함수 호출
    /// </summary>
    /// <param name="skillData">사용된 스킬 데이터</param>
    /// <param name="onComplete">Effect 연산 및 연출이 모두 끝났을 때 Battle Core로 턴을 넘기기 위한 콜백</param>
    /// 단일 타겟 진입점
    public void ExecuteSkillEffects(CEntity caster, CEntity target, CSkillDataSO skillData, Action onComplete)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }
        ExecuteSkillEffects(caster, new List<CEntity> { target }, skillData, onComplete);
    }

    // 광역 스킬 실행 진입점
    public void ExecuteSkillEffects(CEntity caster, IReadOnlyList<CEntity> targets, CSkillDataSO skillData, Action onComplete)
    {
        if (caster == null || targets == null || targets.Count == 0 || skillData == null)
        {
            onComplete?.Invoke();
            return;
        }

        string targetInfo = targets.Count > 1 ? $"{targets[0].name} 외 {targets.Count - 1}명" : targets[0].name;
        CPrint.Log($"{caster.name} 가 {targetInfo} 에게 <{skillData.SkillName}> 실행 시작");

        StartCoroutine(ExecuteEffectsCoroutine(caster, targets, skillData, onComplete));
    }

    private IEnumerator ExecuteEffectsCoroutine(CEntity caster, IReadOnlyList<CEntity> targets, CSkillDataSO skillData, Action onComplete)
    {
        int targetCount = targets.Count;
        CEffectContext[] contexts = new CEffectContext[targetCount];
        bool[] consumedComboInThisSkill = new bool[targetCount];

        Dictionary<EEffectType, bool> sharedCasterTokens = new Dictionary<EEffectType, bool>();

        for (int i=0 ; i < targetCount ; i++)
        {
            contexts[i] = new CEffectContext(caster, targets[i], sharedCasterTokens);
            consumedComboInThisSkill[i] = false;
        }

        if (skillData.EffectList != null && skillData.EffectList.Count > 0)
        {
            foreach (CEffectSO effectData in skillData.EffectList)
            {
                if (effectData == null) continue;

                if (effectData.Strategy == null)
                {
                    CPrint.Warn($"{effectData.Name} 에 연결된 Strategy가 없습니다");
                    continue;
                }

                // 1분기 자신에게 걸 때
                if (effectData.EffectTarget == EEffectTarget.Self)
                {
                    if (IsEntityDead(caster))
                    {
                        CPrint.Log($"대상 ({caster.name})이 사망 상태여서 [{effectData.Name}] 효과를 스킵합니다.");
                        continue;
                    }

                    bool anyHit = false;
                    bool anyCrit = false;
                    bool anyCombo = false;
                    for (int i = 0; i < targetCount; i++)
                    {
                        if (contexts[i].IsHit) anyHit = true;
                        if (contexts[i].IsCrit) anyCrit = true;
                        if (consumedComboInThisSkill[i]) anyCombo = true;
                    }

                    CEffectContext selfContext = new CEffectContext(caster, caster)
                    {
                        IsHit = anyHit || targetCount == 0,
                        IsCrit = anyCrit
                    };

                    if (!CheckEffectCondition(selfContext, effectData, ref anyCombo))
                    {
                        CPrint.Log($"[{effectData.Name} 발동 조건 ({effectData.Condition}) 불총족으로 스킵합니다.]");
                        continue;
                    }

                    effectData.Strategy.Apply(selfContext, effectData);
                    yield return new WaitForSeconds(0.2f);
                }
                // 2분기 대상에게 거는 효과
                else
                {
                    for (int i = 0; i < targetCount; i++)
                    {
                        CEntity currentTarget = targets[i];

                        if (IsEntityDead(currentTarget))
                        {
                            CPrint.Log($"대상 ({currentTarget.name})이 사망 상태여서 [{effectData.Name}] 효과를 스킵합니다.");
                            continue;
                        }

                        if (!CheckEffectCondition(contexts[i], effectData, ref consumedComboInThisSkill[i]))
                        {
                            CPrint.Log($"[{currentTarget.name}]에게 [{effectData.Name} 발동 조건 ({effectData.Condition}) 불충족으로 스킵합니다.]");
                            continue;
                        }

                        effectData.Strategy.Apply(contexts[i], effectData);
                    }

                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        else
        {
            CPrint.Warn($"{skillData.SkillName} 에 등록된 Effect가 없습니다.");
        }

        CPrint.Log($"<{skillData.SkillName}> 연산 및 연출 종료.");

        onComplete?.Invoke();
    }

    private bool CheckEffectCondition(CEffectContext context, CEffectSO effectData, ref bool consumedCombo)
    {
        switch(effectData.Condition)
        {
            case EEffectCondition.None:
                return true;
            case EEffectCondition.TargetHasCombo:
                if (!context.IsHit) return false;
                if (consumedCombo || context.Target.TokenManager.ConsumeToken(EEffectType.Combo))
                {
                    consumedCombo = true;
                    CPrint.Log($"[Combo 연계] [{context.Target.name}]의 콤보 토큰 소모하여 [{effectData.Name}] 발동");
                    return true;
                }
                return false;
            case EEffectCondition.OnCriticalHit:
                return context.IsHit && context.IsCrit;
            case EEffectCondition.CasterIsDeathsDoor:
                return context.Caster != null && context.Caster.IsDeathsDoor;
            default:
                return true;
        }
    }

    private bool IsEntityDead(CEntity entity)
    {
        return entity == null || !entity.isActiveAndEnabled || entity.IsDead;
    }
}
