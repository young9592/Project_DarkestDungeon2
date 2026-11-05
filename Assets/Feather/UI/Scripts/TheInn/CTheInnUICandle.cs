
using UnityEngine;

public class CTheInnUICandle : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _candleUI;
    #endregion

    private string _titleText = "End Expedition";
   
    public void CandleUIOpen()
    {
        _navigationBar.ToggleUI(_candleUI, _titleText);
    }
}
