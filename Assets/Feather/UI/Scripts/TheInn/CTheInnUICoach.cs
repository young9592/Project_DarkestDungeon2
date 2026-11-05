using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTheInnUICoach : MonoBehaviour
{
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _coachUI;
    
    private string _titleText = "The Wainwright";
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CoachUIOpen()
    {
        _navigationBar.ToggleUI(_coachUI, _titleText);
    }
}
