using System.Collections.Generic;
using UnityEngine;

// 미완성입니다.
public abstract class CEntity : MonoBehaviour
{

    /// <summary>
    /// Effect Core 파이프라인 연산이 모두 끝난 최종 데미지만 받아 체력 조절시킴
    /// 힐링 스킬 같은 경우에 음수로 데미지 처리해서 처리할 생각하고 있음
    /// </summary>
    public abstract void TakeDamage(int finalDamage);

    /// <summary>
    /// 출혈, 중독, 콤보 토큰 등 턴 기반의 상태이상 부분
    /// Effect Core에서 명중 판정을 끝내면 여기서 유효한 효과만 던지기
    /// </summary>
    public abstract void ApplyStatusEffect(CEffect statusEffect);

    // TODO : 토큰 흭득 / 차감 등도 필요시 여기에 분리해서 추가 할 수도?
}
