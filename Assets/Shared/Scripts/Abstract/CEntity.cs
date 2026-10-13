using System.Collections.Generic;
using UnityEngine;

// 미완성입니다.
public abstract class CEntity : MonoBehaviour
{


    // 들어오는 데미지도 있지만 속성도 있으면 추가해야합니다.
    public abstract void TakeDamage(int damage, List<CEffect> effectList);
}
