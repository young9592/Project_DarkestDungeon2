
using UnityEngine;

public class CTheInnUIStore : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _storeUI;
    #endregion

    private string _titleText = "물자 관리인";
 
    public void StoreUIOpen()
    {
        _navigationBar.ToggleUI(_storeUI, _titleText);
    }
}
