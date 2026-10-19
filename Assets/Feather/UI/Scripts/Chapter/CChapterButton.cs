
using TMPro;
using UnityEngine;

//챕터 이름 클릭 잠금 상태 관리
public class CChapterButton : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CChapterMenu _chapterMenu;
    [SerializeField] private CChapterAnimation _chapterAnimation;
    //챕터 텍스트
    [SerializeField] private TMP_Text _chapterText;

    [SerializeField] private int _chapterIndex;

    //눌렀을 시 변하는 글씨
    [SerializeField] private GameObject _signature;
    //시작 글씨
    [SerializeField] private GameObject _signatureWhite;
    //눌렀을 시 이펙트
    [SerializeField] private GameObject _signatureEffect;
    #endregion

    //텍스트를 눌렀을때
    public void OnClick()
    {
        _chapterMenu.SelectChapter(_chapterIndex);
        _signatureWhite.SetActive(true);
    }

    //글씨를 눌렀을때
    public void SignatureClick()
    {
        _signatureEffect.SetActive(true);
        _signature.SetActive(true);
        _signatureWhite.SetActive(false);
        _chapterAnimation.ChapterMenuExit();
        //게임 시작 
    }

    //글씨 초기화
    public void ResetSignature()
    {
        _signature.SetActive(false);
        _signatureWhite.SetActive(false);
        _signatureEffect.SetActive(false);
    }

}
