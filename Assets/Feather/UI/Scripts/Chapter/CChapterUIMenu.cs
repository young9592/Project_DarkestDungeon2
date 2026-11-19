
using TMPro;
using UnityEngine;

public class CChapterUIMenu : MonoBehaviour
{
    #region Inspector
    [SerializeField] private TMP_Text _chapterTextButtons;
    [SerializeField] private GameObject _chapterProfile;
    #endregion

    public void SelectChapter()
    {
        _chapterProfile.SetActive(true);
    }
    public void ResetChapterProfile()
    {
        _chapterProfile.SetActive(false);
    }
}
