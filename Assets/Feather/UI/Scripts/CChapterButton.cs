using TMPro;
using UnityEngine;
using UnityEngine.UI;

//버튼 클릭 처리 자신이 선택됐을 때 메뉴에 알림
public class CChapterButton : MonoBehaviour
{
    #region Inspector
    //챕터가 열릴때마다 갱신용
    [SerializeField] private TMP_Text[] _chapterTextButton = new TMP_Text[5];

    //시작 글씨
    [SerializeField] private GameObject _signature;
    [SerializeField] private GameObject _signatureWhite;
    [SerializeField] private GameObject _signatureEffect;

    // 화면 전환
    [SerializeField] private GameObject _mainMenuA;             // 3개 : 고해,왕국,모드 버튼들
    [SerializeField] private GameObject _mainMenuB;             // 2개 : 고해이어하기,새 고해 버튼들
    [SerializeField] private GameObject _confessionButton;
    [SerializeField] private GameObject _backButton;            // 뒤로가기 버튼
    // 게임종료도 만들예정

    //투명도
    [SerializeField, Range(0f, 1f)]
    private float _alpha = 0.5f;
    #endregion

    #region 
    // 초반 튜토리얼을 눌렀는가?
    private bool _pausdPrologue = false;
    #endregion

    void Start()
    {
        FirstUISet();
    }

    void Update()
    {
       
    }

    public void OnClickConfession()     // 고해 버튼 클릭
    {
        if (_mainMenuA != null)
        {
            _mainMenuA.SetActive(false); 
        }
        if (_mainMenuB != null)
        {
            _mainMenuB.SetActive(true); 
        }
        Debug.Log("고해버튼 눌림");
    }
    public void OnClickback()           // 뒤로가기 눌렀을때
    {
        if (_mainMenuA != null)
        {
            _mainMenuA.SetActive(true);
        }
        if (_mainMenuB != null)
        {
            _mainMenuB.SetActive(false);
        }
        Debug.Log("뒤로가기 눌림");
    }
   
    private void FirstUISet()           // 처음 상태로 전환
    {
        if (_mainMenuA != null)
        {
            _mainMenuA.SetActive(true);
        }
        if (_mainMenuB != null)
        {
            _mainMenuB.SetActive(false);
        }
        Debug.Log("처음 타이틀 화면(고해,왕국,모드)");
    }
    public void OnClickExit()           // 게임 종료버튼
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = false;
#else
        Application.Quit();
#endif
        Debug.Log("게임종료버튼 눌림");
    }
    public void ClickTextButton()
    {

    }

    public void GameStartUI()
    {

    }
}
