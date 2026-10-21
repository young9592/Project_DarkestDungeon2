
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//메뉴 등장 퇴장 애니메이션 및 전체 메뉴 상태 관리
public class CChapterUIMenu : MonoBehaviour
{
    #region Inspector
    //버튼
    [SerializeField] private GameObject[] _chapterButton = new GameObject[5];
    //택스트
    [SerializeField] private TMP_Text[] _chapterTextButtons;

    [SerializeField] private Button[] _textButtons;
    //택스트를 눌렀을떄 뜨는 버튼 프로필
    [SerializeField] private GameObject[] _chapterProfile = new GameObject[5];
    #endregion

    //챕터 클리어에 따라서 UI 갱신
    public void UpdateChapterUI(bool[] unlockedChapters)
    {
        for (int i = 0; i < _chapterButton.Length; i++)
        {
            //챕터 오픈
            _chapterButton[i].SetActive(true);
            //잠긴 챕터는 클릭 불가능
            _textButtons[i].interactable = unlockedChapters[i];
            //열린 챕터 없으면 ??? 로
            if (!unlockedChapters[i])
            {
                _chapterTextButtons[i].text = "???";
            }
        }
    }

    //챕터 선택
    public void SelectChapter(int index)
    {
        for (int i = 0; i < _chapterProfile.Length; i++)
        {
            _chapterProfile[i].SetActive(i == index);
        }
    }
}
