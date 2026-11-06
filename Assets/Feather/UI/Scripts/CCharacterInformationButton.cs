using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterInformationButton : MonoBehaviour
{
    #region Inspector
    [Header("캐릭터 정보 아이콘 버튼")]
    [SerializeField] private GameObject _relationshipsInforma;
    [SerializeField] private GameObject _combatSkillsButton;
    [SerializeField] private GameObject _conditionsButton;
    [SerializeField] private GameObject _storyButton;

    [Header("캐릭터 정보 창")]
    [SerializeField] private GameObject _relsInformations;              // Rels : Relationships
    [SerializeField] private GameObject _cmbSklInformations;            // CmbSkl : Combat Skills
    [SerializeField] private GameObject _cndInformations;               // CND : Conditions 
    [SerializeField] private GameObject _styInformations;               // STY : Story
    #endregion
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    #region Icon Button
    public void OnClickRelationshipsBtn()    // 관계 아이콘버튼
    {
        Debug.Log("Rel Icon Button눌림");
    }
    public void OnClickCombatSkillsBtn()    // 전투기술 아이콘버튼
    {
        Debug.Log("CbS Icon Button눌림");
    }
    public void OnClickConditionsBtn()    // 상태 아이콘버튼
    {
        Debug.Log("Cdt Icon Button눌림");
    }
    public void OnClickStoryBtn()    // 이야기 아이콘버튼
    {
        Debug.Log("Sty Icon Button눌림");
    } 
    #endregion
}
