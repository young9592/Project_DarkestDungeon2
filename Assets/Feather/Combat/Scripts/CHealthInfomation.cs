
using UnityEngine;
using UnityEngine.EventSystems;

public class CHealthInfomation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Inspector
    [Header("Health Infomation Box")]
    [SerializeField] private GameObject _tooltip;

    [Header("Connect Health")]
    [SerializeField] private CEntityState.EEntityState _entityState;
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _currentStress;
    [SerializeField] private int _maxStress;
    #endregion

    #region Property
    public bool IsInitialize => _tooltip != null;
    #endregion

    public void Initialize(GameObject tooltip, CEntityState.EEntityState entityState)
    {
        _tooltip = tooltip;
        _entityState = entityState;

        if (CPrint.NullUnityObjectCheck(gameObject.name, _tooltip))
        {
            enabled = false;
            return;
        }
    }
    public void SetHealth(int currentHealth, int maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = maxHealth;
    }
    public void SetStress(int currentStress, int maxStress)
    {
        _currentStress = currentStress;
        _maxStress = maxStress;
    }
    public void ResetDot()
    {
        _currentHealth = 0;
        _maxHealth = 0;
        _currentStress = 0;
        _maxStress = 0;
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip == null)
        {
            return;
        }

        if (!_tooltip.activeSelf)
        {
            _tooltip.SetActive(true);
        }

        Transform tr = _tooltip.GetComponent<Transform>();
        Transform moveTr = gameObject.GetComponent<Transform>();

        tr.position = moveTr.position;


        CHealthText healthText = _tooltip.GetComponent<CHealthText>();

        switch (_entityState)
        {
            case CEntityState.EEntityState.Hero:
                healthText.SetHealthInfomation(_currentHealth, _maxHealth, _currentStress, _maxStress);
                break;
            case CEntityState.EEntityState.Enemy:
                healthText.SetHealthInfomation(_currentHealth, _maxHealth);
                break;
            case CEntityState.EEntityState.Enemy_Corpse:
                healthText.SetHealthInfomation(_currentHealth, _maxHealth);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip == null)
        {
            return;
        }

        if (_tooltip.activeSelf)
        {
            _tooltip.SetActive(false);
        }
    }
}
