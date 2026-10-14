
using TMPro;
using UnityEngine;

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

    //투명도
    [SerializeField, Range(0f, 1f)]
    private float _alpha =0.5f;
    #endregion

    #region 
    // 초반 튜토리얼을 눌렀는가?
    private bool _pausdPrologue = false;
    #endregion
    private void Awake()
    {
        
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

  
    private void FirstUISet()
    {
        
    }
    public void ClickTextButton()
    {
       
    }
    
    public void GameStartUI()
    {

    }
}
