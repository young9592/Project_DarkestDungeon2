
using UnityEngine;

public class CTheInnUIStore : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _storeUI;
    #endregion

    private string _titleText = "The Provisioner";
 
    public void StoreUIOpen()
    {
        _navigationBar.ToggleUI(_storeUI, _titleText);
    }
}
