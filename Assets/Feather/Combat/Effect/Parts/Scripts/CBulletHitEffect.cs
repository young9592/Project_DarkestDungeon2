using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CBulletHitEffect : CSkillEffectPart
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _tr;

    [Header("Option")]
    [SerializeField] private float _delay = 1.5f;
    [SerializeField] List<Sprite> _randomEffects;
    #endregion

    #region Field
    private Color _initColor;
    private float _nextTime;
    #endregion

    private void Awake()
    {
        if (_image == null || _tr == null || _randomEffects == null || _randomEffects.Count == 0)
        {
            CPrint.Error($"{gameObject.name} : Effect Null Find");
            enabled = false;
            return;
        }

        _initColor = _image.color;
        _nextTime = Time.time + _delay;
        _image.sprite = _randomEffects[Random.Range(0, _randomEffects.Count)];
    }

    private void Update()
    {
        float t = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);
        _image.color = new Color(_initColor.r, _initColor.g, _initColor.b, Mathf.Lerp(_initColor.a, 0, t));

        _tr.Translate(Vector3.right * _moveSpeed * Time.deltaTime, Space.World);
    }
}
