using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSkillEffectPart : MonoBehaviour
{
    #region Inspector
    [Header("Option")]
    [SerializeField] protected float _moveSpeed;
    #endregion

    public void SetMultipleSpeed(float multiply)
    {
        _moveSpeed *= multiply;
    }

    public void ChangeDirection()
    {
        _moveSpeed *= -1;
    }
}
