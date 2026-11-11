using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterInformationButton : MonoBehaviour
{
    #region Inspector
    [Header("캐릭터 정보창")]
    [SerializeField] private GameObject _characterInfoTilte;
    



    #endregion
    void Start()
    {
        
    }

    void Update()
    {
        InputQ();
    }

    #region Q Input
    public void InputQ()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q 눌림");
        }
    }
    #endregion
}
