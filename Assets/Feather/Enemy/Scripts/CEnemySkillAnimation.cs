using System;
using UnityEngine;

[Serializable]
public class CEnemySkillAnimation
{
    [SerializeField] private CSkillDataSO _skill;
    [Tooltip("Animator의 전체 상태 경로를 입력합니다.")]
    [SerializeField] private string _attackState = "Base Layer.Attack";
    [Tooltip("이 상태의 재생이 끝나면 공격 완료를 알립니다.")]
    [SerializeField] private string _completionState = "Base Layer.AttackPose";
    [SerializeField] private CEnemySkillFeedbackSO _feedback;

    [Header("Source Timeline")]
    [SerializeField] private bool _useSourceTiming;
    [SerializeField, Min(0.0f)] private float _attackClipStartTime;
    [SerializeField, Min(0.0f)] private float _recoveryStartDelay;
    [SerializeField, Min(0.0f)] private float _recoveryClipStartTime;
    [SerializeField, Min(0.0f)] private float _recoveryBlendTime;

    [SerializeField, Min(0.0f)] private float _recoveryClipEndTime;

    [SerializeField, Min(0.0f)] private float _recoveryIdleBlendTime;

    public float RecoveryIdleBlendTime { get { return _recoveryIdleBlendTime; } }
    public float RecoveryClipEndTime { get { return _recoveryClipEndTime; } }
    public bool UseSourceTiming { get { return _useSourceTiming; } }
    public float AttackClipStartTime { get { return _attackClipStartTime; } }
    public float RecoveryStartDelay { get { return _recoveryStartDelay; } }
    public float RecoveryClipStartTime { get { return _recoveryClipStartTime; } }
    public float RecoveryBlendTime { get { return _recoveryBlendTime; } }

    public CEnemySkillFeedbackSO Feedback
    {
        get { return _feedback; }
    }

    public CSkillDataSO Skill
    {
        get { return _skill; }
    }

    public string AttackState
    {
        get { return _attackState; }
    }

    public string CompletionState
    {
        get { return _completionState; }
    }
}
