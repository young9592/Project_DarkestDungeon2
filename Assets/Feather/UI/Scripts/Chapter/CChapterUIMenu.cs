
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
    //챕터 기본 이름
    [SerializeField] private string[] _chapterText = new string[5];

    [SerializeField] private Button[] _textButtons;
    //택스트를 눌렀을떄 뜨는 버튼 프로필
    [SerializeField] private GameObject[] _chapterProfile = new GameObject[5];
    #endregion


    private void Start()
    {
        ShowPrologue();
    }
    //챕터 보여주기
    public void ShowChapterSelect(bool[] unlockedChapters)
    {
        for(int i =0; i < _chapterButton.Length; i++)
        {
            _chapterButton[i].SetActive(true);
            _textButtons[i].interactable = unlockedChapters[i];

            _chapterTextButtons[i].text =
                unlockedChapters[i] ? _chapterText[i] : "???";
        }
    }
    //프롤로그 보여주기
    public void ShowPrologue()
    {
        for(int i =0; i< _chapterButton.Length; i++)
        {
            _chapterButton[i].SetActive(i == 0);
        }
        _chapterTextButtons[0].text = "Prologue";
    }

    //챕터 선택
    public void SelectChapter(int index)
    {
        for (int i = 0; i < _chapterProfile.Length; i++)
        {
            _chapterProfile[i].SetActive(i == index);
        }
    }
    public void ResetChapterProfile()
    {
        for (int i = 0; i < _chapterProfile.Length; i++)
        {
            _chapterProfile[i].SetActive(false);
        }
    }

}
