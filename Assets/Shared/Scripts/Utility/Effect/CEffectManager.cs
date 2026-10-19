using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public void ExecuteSkillEffects(CEntity caster, CEntity target, CSkillDataSO skillData, Action onComplete)
    {
        CPrint.Log($"{caster.name} 가 {target.name} 에게 <{skillData.SkillName}> 실행 시작");

        // 이펙트 연출 시간 기다리도록 처리
        StartCoroutine(ExecuteEffectsCoroutine(caster, target, skillData, onComplete));
    }

    private IEnumerator ExecuteEffectsCoroutine(CEntity caster, CEntity target, CSkillDataSO skillData, Action onComplete)
    {
        CEffectContext context = new CEffectContext(caster, target);

        if (skillData.EffectList != null && skillData.EffectList.Count > 0)
        {
            foreach (CEffectSO effectData in skillData.EffectList)
            {
                // 미리 할당한 전략이 있다면 실행
                if (effectData.Strategy != null)
                {
                    effectData.Strategy.Apply(context, effectData);

                    // TODO : 연출(파티클, 사운드 등) 대기 시간이 필요시 여기서 yield return 처리
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    CPrint.Warn($"{effectData.Name} 에 연결된 Strategy가 없습니다");
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

}
