using UnityEngine;

/// <summary>
/// Effect의 실제 작동 로직 구현하는 추상 클래스
/// </summary>
public abstract class CEffectStrategy : ScriptableObject
{
    public abstract void Apply(CEffectContext context, CEffectSO effectData);
}
