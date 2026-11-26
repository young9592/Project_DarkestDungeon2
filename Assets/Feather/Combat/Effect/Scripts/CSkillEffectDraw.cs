using System.Collections.Generic;
using UnityEngine;

public class CSkillEffectDraw : MonoBehaviour
{
    #region Inspector
    [Header("Effect")]
    [SerializeField] private List<GameObject> _effectList;

    [Header("Option")]
    [SerializeField] private float _destroyTime = 2f;
    #endregion

    private void Awake()
    {
        for (int i = 0; i < _effectList.Count; i++)
        {
            if (_effectList[i] == null)
            {
                CPrint.Error($"{gameObject.name} : Effect Null Find.");
                enabled = false;
                return;
            }
        }

        Destroy(gameObject, _destroyTime);
    }

    public void SetRiposteSpeed(float multiply)
    {
        for (int i = 0; i < _effectList.Count; i++)
        {
            CSkillEffectPart skillEffectPart = _effectList[i].GetComponent<CSkillEffectPart>();

            skillEffectPart.SetMultipleSpeed(multiply);
        }
    }

    public void ChangeDirection()
    {
        for(int i = 0; i < _effectList.Count; i++)
        {
            CSkillEffectPart skillEffectPart = _effectList[i].GetComponent<CSkillEffectPart>();

            skillEffectPart.ChangeDirection();
        }
    }
}
