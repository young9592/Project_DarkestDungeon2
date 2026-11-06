using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CCombatDebugMenu : MonoBehaviour
{
    #region Inspector
    [Header("MessageManager")]
    [SerializeField] private CCombatFloatingMessageManager _messageManager;

    [Header("Debug Menu")]
    [SerializeField] private CBasicCombatManager _combatManager;

    [SerializeField] private GameObject _debugRoot;
    [SerializeField] private TMP_Dropdown _selectEntity;
    [SerializeField] private TMP_Dropdown _selectEffect;
    [SerializeField] private Button _shootEffectBtn;

    [Header("Test Effect")]
    [SerializeField] private List<CEffectSO> _effectSOList;
    #endregion

    #region Field
    // Debug Menu Value
    private Dictionary<string, EEffectType> _applyOptionList = new Dictionary<string, EEffectType>();
    private Dictionary<EEffectType, CEffectSO> _effectList = new Dictionary<EEffectType, CEffectSO>();
    private int _selecedtEffectIdx = 0;
    private int _selectedEntitytIdx = 0;
    #endregion



    private void Awake()
    {
        _applyOptionList.Add("데미지", EEffectType.Damage);
        _applyOptionList.Add("회복", EEffectType.Heal);
        _applyOptionList.Add("스트레스", EEffectType.None);
        _applyOptionList.Add("스트레스 회복", EEffectType.None);
        _applyOptionList.Add("출혈", EEffectType.Bleed);
        _applyOptionList.Add("중독", EEffectType.Blight);
        _applyOptionList.Add("화상", EEffectType.Burn);
        _applyOptionList.Add("현기증", EEffectType.Daze);
        _applyOptionList.Add("기절", EEffectType.Stun);
        _applyOptionList.Add("추가행동", EEffectType.None);
        _applyOptionList.Add("속도", EEffectType.Speed);

        for (int i = 0; i < _effectSOList.Count; i++)
        {
            _effectList.Add(_effectSOList[i].EffectType, _effectSOList[i]);
        }
    }

    private void OnEnable()
    {
        _selectEffect.onValueChanged.AddListener(index => OnDropboxChanged("Effect", index));
        _selectEntity.onValueChanged.AddListener(index => OnDropboxChanged("Entity", index));
        _shootEffectBtn.onClick.AddListener(OnEffectApplyButton);
    }

    private void OnDisable()
    {
        _selectEffect.onValueChanged.RemoveAllListeners();
        _selectEntity.onValueChanged.RemoveAllListeners();
        _shootEffectBtn.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        // Debug Menu Key
        DebugMenuKey();
    }

    public void Initialize()
    {
        _selectEffect.ClearOptions();

        var keys = _applyOptionList.Keys;

        List<string> options = new List<string>();

        foreach (var key in keys)
        {
            options.Add(key);
        }

        _selectEffect.AddOptions(options);
        _selectEffect.RefreshShownValue();

        SetDropboxEntityList();
    }

    private void DebugMenuKey()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            _debugRoot.SetActive(!_debugRoot.activeSelf);
        }
    }
    // 디버그용 엔티티 드롭박스
    public void SetDropboxEntityList()
    {
        _selectEntity.ClearOptions();

        List<string> options = new List<string>();
        List<(int id, CEntity entity)> entities = _combatManager.EntityList;

        for (int i = 0; i < entities.Count; i++)
        {
            options.Add(entities[i].entity.name);
        }
        _selectEntity.AddOptions(options);

        _selectEntity.RefreshShownValue();
        _selectEntity.value = 0;
    }
    // 디버그용 드랍박스 변경 리스너
    private void OnDropboxChanged(string dropboxName, int index)
    {
        if (dropboxName == "Effect")
        {
            _selecedtEffectIdx = index;
        }
        else if (dropboxName == "Entity")
        {
            _selectedEntitytIdx = index;
        }
    }
    // 디버그용 버튼 리스너
    private void OnEffectApplyButton()
    {
        List<(int id, CEntity entity)> entities = _combatManager.EntityList;
        Dictionary<CEntity, CEntityState> entityStates = _combatManager.EntityStateList;

        string applyOptionText = _selectEffect.options[_selecedtEffectIdx].text;
        EEffectType applyOption = _applyOptionList[applyOptionText];
        CEntity selectEntity = entities[_selectedEntitytIdx].entity;

        switch (applyOption)
        {
            case EEffectType.Damage:
                int rand = Random.Range(0, 10);
                int damage = Random.Range(1, 5);

                if (rand < 6)
                {
                    selectEntity.TakeDamage(damage);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Damage, $"{damage}");
                }
                else
                {
                    selectEntity.TakeDamage(damage * 2);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Damage_Crit, $"{damage * 2}");
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Damage_Crit_Msg, "치명타!");
                }


                if (selectEntity.IsDeathsDoor)
                {
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Death_Door, "죽음의 문턱!");
                }
                break;
            case EEffectType.Heal:
                rand = Random.Range(0, 10);
                int heal = Random.Range(1, 5);

                if (rand < 6)
                {
                    selectEntity.TakeDamage(-heal);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Heal, $"{heal}");
                }
                else
                {
                    selectEntity.TakeDamage(-heal * 2);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Heal, $"{heal * 2}");
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Heal_Crit_Msg, "치명타!");
                }
                break;
            case EEffectType.Bleed:
            case EEffectType.Blight:
            case EEffectType.Burn:
            case EEffectType.Daze:
            case EEffectType.Stun:
            case EEffectType.Speed:
                CEffect effect = new CEffect(_effectList[applyOption]);
                selectEntity.ApplyStatusEffect(effect);
                break;
            // 추가행동
            case EEffectType.None:
                int stress = Random.Range(1, 4);

                if (applyOptionText == "스트레스")
                {
                    selectEntity.ModifyStress(stress);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Stress, $"{stress}");
                    _messageManager.ShowIcon(selectEntity.gameObject, CCombatFloatingIcon.IconType.Stress);
                }
                else if (applyOptionText == "스트레스 회복")
                {
                    selectEntity.ModifyStress(-stress);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Stress, $"{stress}");
                    _messageManager.ShowIcon(selectEntity.gameObject, CCombatFloatingIcon.IconType.StressHealing);
                }
                else
                {
                    selectEntity.SetExtraAction(true);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.None, "추가 행동 부여");
                }

                break;
        }

        Debug.Log("속성 부여 완료");
    }
}
