using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//메뉴 등장 퇴장 애니메이션 및 전체 메뉴 상태 관리
public class CChapterMenu : MonoBehaviour
{
    #region Inspector
    [SerializeField] private GameObject[] _chapterButton = new GameObject[5];
    [SerializeField] private GameObject[] _chapterProfile = new GameObject[5];
    #endregion

    public void UpdateChapterUI(bool[] unlockedChapters)
    {
        for(int i = 0; i< _chapterButton.Length; i++)
        {
            _chapterButton[i].SetActive(unlockedChapters[i]);
        }
    }
    
    public void SelectChapter(int index)
    {
        for (int i = 0;i < _chapterProfile.Length; i++)
        {
            _chapterProfile[i].SetActive(i == index);
        }
    }
}
