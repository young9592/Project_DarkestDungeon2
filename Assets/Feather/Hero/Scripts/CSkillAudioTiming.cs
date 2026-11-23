using System;
using UnityEngine;

[Serializable]
public class CSkillAudioTiming
{
    [Tooltip("준비 모션 기준 지연(초). 사용 모션에 들어가면 남은 준비음 예약은 취소됩니다.")]
    [SerializeField, Min(0.0f)] private float _preparationDelay;
    [Tooltip("사용 모션과 명중 결과가 준비된 뒤 기다릴 시간(초). 행동 종료 시 남은 예약은 취소됩니다.")]
    [SerializeField, Min(0.0f)] private float _useDelay;
    [Tooltip("빗나감 판정과 사용 모션이 준비된 뒤 기다릴 시간(초).")]
    [SerializeField, Min(0.0f)] private float _missDelay;
    [Tooltip("복귀 기준 지연(초). 사용음 대기 중이면 사용음 뒤에 계산합니다. 새 스킬이나 취소 시 예약을 해제합니다.")]
    [SerializeField, Min(0.0f)] private float _returnDelay;

    public float GetDelay(EHeroSkillSoundPhase phase)
    {
        switch (phase)
        {
            case EHeroSkillSoundPhase.Preparation: return Mathf.Max(0.0f, _preparationDelay);
            case EHeroSkillSoundPhase.Use: return Mathf.Max(0.0f, _useDelay);
            case EHeroSkillSoundPhase.Miss: return Mathf.Max(0.0f, _missDelay);
            case EHeroSkillSoundPhase.Return: return Mathf.Max(0.0f, _returnDelay);
            default: return 0.0f;
        }
    }
}
