using UnityEngine;
using UnityEngine.UI;

public class CSlashSharpEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private MeshRenderer _mesh;
    [SerializeField] private Transform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    #endregion

    #region Field
    private float _nextTime;
    #endregion

    private void Awake()
    {
        if (_mesh == null || _tr == null)
        {
            CPrint.Error($"{gameObject.name} : Effect Null Find");
            enabled = false;
            return;
        }

        _nextTime = Time.time + _delay;
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);

        _mesh.material.SetFloat("_Prograss", t * 1.05f);
        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, Space.World);
    }
}
