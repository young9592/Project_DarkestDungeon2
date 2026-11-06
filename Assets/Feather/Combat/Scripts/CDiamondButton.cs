using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CDiamondButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public enum ButtonType
    {
        Pass,
        RankChange
    }

    #region Inspector
    [Header("Connect")]
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private Image _backEffect;

    [Header("Option")]
    [SerializeField] private ButtonType _buttonType = ButtonType.Pass;
    [SerializeField] private float _delay = 3f;
    #endregion

    #region Field
    private bool _isPress = false;
    private float _nextTime = 0f;
    private Coroutine _pressCoroutine = null;
    #endregion

    private void Awake()
    {
        if (_combatUI == null)
        {
            CPrint.Warn($"{gameObject.name} Connect is null.");
            enabled = false;
            return;
        }

        if (_buttonType == ButtonType.Pass && _backEffect == null)
        {
            CPrint.Warn($"{gameObject.name} Connect is null.");
            enabled = false;
            return;
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isPress)
        {
            return;
        }

        if(_pressCoroutine != null)
        {
            return;
        }

        if (_buttonType == ButtonType.Pass)
        {
            if (!_isPress)
            {
                _nextTime = Time.time + _delay;
                _combatUI.OnPressActionPass();
                _pressCoroutine = StartCoroutine(CoDelay());
                _isPress = true;
            }
        }
        else
        {
            _combatUI.OnPressRankChange();
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_buttonType == ButtonType.RankChange)
        {
            return;
        }

        if (!_isPress)
        {
            return;
        }

        _backEffect.fillAmount = 0;
        StopCoroutine(_pressCoroutine);
        _pressCoroutine = null;
        _isPress = false;
    }

    private IEnumerator CoDelay()
    {
        while (_nextTime > Time.time)
        {
            _backEffect.fillAmount = Mathf.Clamp01(1f - (_nextTime - Time.time) / _delay);
            yield return null;
        }

        _combatUI.ActionPass();
        _backEffect.fillAmount = 0;
        _isPress = false;
        _pressCoroutine = null;
    }
}
