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
