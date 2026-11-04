
using UnityEngine;

public class CTheInnUIJournal : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _journalUI;
    #endregion

    private string _titleText = "Torch and Crown";
   
    public void JournalUIOpen()
    {
        _navigationBar.ToggleUI(_journalUI, _titleText);
    }
}
