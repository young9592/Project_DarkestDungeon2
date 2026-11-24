using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTheInnUICoach : MonoBehaviour
{
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _coachUI;
    
    private string _titleText = "마차 관리인";
  
    public void CoachUIOpen()
    {
        _navigationBar.ToggleUI(_coachUI, _titleText);
    }
}
