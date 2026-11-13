using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CCombatDebugMenu : MonoBehaviour
{
    #region Inspector
    [Header("MessageManager")]
    [SerializeField] private CCombatManager _combatManager;
    [SerializeField] private CCombatUI _combatUI;
    [SerializeField] private CCombatFloatingMessageManager _messageManager;

    [Header("Debug Menu")]
    [SerializeField] private GameObject _debugRoot;
    [SerializeField] private TMP_Dropdown _selectEntity;
    [SerializeField] private TMP_Dropdown _selectEffect;
    [SerializeField] private Button _shootEffectBtn;

    [Header("Test Effect")]
    [SerializeField] private List<CEffectSO> _effectSOList;

    [Header("Option")]
    [SerializeField] private bool _debugMode = false;
    #endregion

    #region Field
    // Debug Menu Value
    private Dictionary<string, EEffectType> _applyOptionList = new Dictionary<string, EEffectType>();
    private Dictionary<EEffectType, CEffectSO> _effectList = new Dictionary<EEffectType, CEffectSO>();
    private int _selecedtEffectIdx = 0;
    private int _selectedEntitytIdx = 0;
    private  List<Object> _checkList = new List<Object>();
    #endregion

    private void Awake()
    {
        if (!_debugMode)
        {
            return;
        }
        
        _checkList.Add(_combatManager);
        _checkList.Add(_combatUI);
        _checkList.Add(_messageManager);
        _checkList.Add(_debugRoot);
        _checkList.Add(_selectEntity);
        _checkList.Add(_selectEffect);
        _checkList.Add(_shootEffectBtn);

        if (_effectSOList.Count == 0)
        {
            CPrint.Warn("리스트가 누락되었습니다.");
            enabled = false;
            return;
        }

        for(int i = 0; i < _effectSOList.Count; i++)
        {
            _checkList.Add(_effectSOList[i]);
        }

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
        {
            enabled = false;
            return;
        }

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
        _applyOptionList.Add("고정", EEffectType.Immobilize);
        _applyOptionList.Add("반격", EEffectType.Riposte);
        _applyOptionList.Add("은신", EEffectType.Stealth);
        _applyOptionList.Add("콤보", EEffectType.Combo);

        for (int i = 0; i < _effectSOList.Count; i++)
        {
            _effectList.Add(_effectSOList[i].EffectType, _effectSOList[i]);
        }
    }

    private void OnEnable()
    {
        if (!_debugMode)
        {
            return;
        }

        _selectEffect.onValueChanged.AddListener(index => OnDropboxChanged("Effect", index));
        _selectEntity.onValueChanged.AddListener(index => OnDropboxChanged("Entity", index));
        _shootEffectBtn.onClick.AddListener(OnEffectApplyButton);
    }

    private void OnDisable()
    {
        if (!_debugMode)
        {
            return;
        }

        _selectEffect.onValueChanged.RemoveAllListeners();
        _selectEntity.onValueChanged.RemoveAllListeners();
        _shootEffectBtn.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        if (!_debugMode)
        {
            return;
        }

        // Debug Menu Key
        DebugMenuKey();
    }

    public void Initialize()
    {
        if (!_debugMode)
        {
            return;
        }

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
        if (!_debugMode)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            _debugRoot.SetActive(!_debugRoot.activeSelf);
        }
    }
    // 디버그용 엔티티 드롭박스
    public void SetDropboxEntityList()
    {
        if (!_debugMode)
        {
            return;
        }

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
        if (!_debugMode)
        {
            return;
        }


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
        if (!_debugMode)
        {
            return;
        }


        List<(int id, CEntity entity)> entities = _combatManager.EntityList;
        IReadOnlyDictionary<CEntity, CEntityState> entityStates = _combatUI.EntitiesState;

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
                    if (!selectEntity.IsDead)
                    {
                        selectEntity.TakeDamage(damage);

                        if (selectEntity is CEnemy && selectEntity.IsDead)
                        {
                            if (entityStates.ContainsKey(selectEntity))
                            {
                                _combatUI.ReconnectStateUI(selectEntity);
                            }
                        }
                    }
                    else
                    {
                        if (selectEntity is CPlayer)
                        {
                            return;
                        }
                        else
                        {
                            (selectEntity as CEnemy).Corpse.TakeDamage(damage);
                        }
                    }
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Damage, $"{damage}");
                }
                else
                {
                    if (!selectEntity.IsDead)
                    {
                        selectEntity.TakeDamage((int)(damage * 1.5f));

                        if (selectEntity is CEnemy && selectEntity.IsDead)
                        {
                            if (_combatUI.EntitiesState.ContainsKey(selectEntity))
                            {
                                _combatUI.ReconnectStateUI(selectEntity);
                            }
                        }
                    }
                    else
                    {
                        if (selectEntity is CPlayer)
                        {
                            return;
                        }
                        else
                        {
                            (selectEntity as CEnemy).Corpse.TakeDamage((int)(damage * 1.5f));
                        }
                    }
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Damage_Crit, $"{(int)(damage * 1.5f)}");
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
            case EEffectType.Immobilize:
            case EEffectType.Riposte:
            case EEffectType.Stealth:
            case EEffectType.Combo:
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
                    _messageManager.ShowIcon(selectEntity.gameObject, CIcon.IconType.Stress);
                }
                else if (applyOptionText == "스트레스 회복")
                {
                    selectEntity.ModifyStress(-stress);
                    _messageManager.ShowMessage(selectEntity.gameObject, CCombatFloatingText.EMessageType.Stress, $"{stress}");
                    _messageManager.ShowIcon(selectEntity.gameObject, CIcon.IconType.StressHeal);
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
